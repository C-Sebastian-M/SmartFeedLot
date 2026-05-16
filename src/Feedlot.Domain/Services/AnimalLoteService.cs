using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.Interfaces;

namespace Feedlot.Domain.Services;

/// <summary>
/// Domain Service que coordina la lógica de movimiento de animales entre lotes.
/// 
/// ¿Por qué Domain Service y no método en el Aggregate?
/// La invariante "un animal no puede estar en dos lotes activos simultáneamente"
/// requiere consultar el estado de AMBOS aggregates (Animal y Lote).
/// Un Aggregate no puede acceder a otro Aggregate directamente — eso violaría
/// los límites de consistencia. El Domain Service tiene acceso a ambos repositorios
/// y puede coordinar la operación de forma segura.
/// </summary>
public sealed class AnimalLoteService
{
    private readonly IAnimalRepository _animalRepo;
    private readonly ILoteRepository _loteRepo;

    public AnimalLoteService(IAnimalRepository animalRepo, ILoteRepository loteRepo)
    {
        _animalRepo = animalRepo;
        _loteRepo = loteRepo;
    }

    /// <summary>
    /// Mueve un animal de su lote actual a un lote destino.
    /// Aplica la invariante de pertenencia única: verifica que el animal
    /// no esté ya activo en otro lote antes de moverlo.
    /// </summary>
    public async Task MoverAnimalAsync(
        Guid animalId,
        Guid loteDestinoId,
        DateOnly fechaMovimiento,
        MotivoMovimiento motivo,
        CancellationToken ct = default)
    {
        var animal = await _animalRepo.ObtenerPorIdAsync(animalId, ct)
            ?? throw new DomainException($"Animal con ID '{animalId}' no encontrado.");

        if (!animal.EstaActivo)
            throw new AnimalInactivoException(animalId);

        var loteDestino = await _loteRepo.ObtenerPorIdAsync(loteDestinoId, ct)
            ?? throw new DomainException($"Lote destino con ID '{loteDestinoId}' no encontrado.");

        if (!loteDestino.EstaActivo)
            throw new DomainException(
                $"El lote destino '{loteDestinoId}' no está activo. No puede recibir animales.");

        // Verificar invariante: animal en un solo lote activo.
        var loteOrigen = await _loteRepo.ObtenerLoteActivoDelAnimalAsync(animalId, ct);

        if (loteOrigen is not null)
        {
            if (loteOrigen.Id == loteDestinoId)
                throw new DomainException(
                    $"El animal '{animalId}' ya se encuentra en el lote destino '{loteDestinoId}'.");

            // Retirar del lote origen antes de agregar al destino.
            loteOrigen.RetirarAnimal(animalId, fechaMovimiento, motivo);
            _loteRepo.Actualizar(loteOrigen);
        }

        // Agregar al lote destino.
        loteDestino.AgregarAnimal(animalId, fechaMovimiento, motivo);
        _loteRepo.Actualizar(loteDestino);
    }

    /// <summary>
    /// Ingresa un animal por primera vez a un lote.
    /// Valida que el animal no esté ya en ningún lote activo.
    /// </summary>
    public async Task IngresoInicialAsync(
        Guid animalId,
        Guid loteId,
        DateOnly fechaIngreso,
        CancellationToken ct = default)
    {
        var loteActivo = await _loteRepo.ObtenerLoteActivoDelAnimalAsync(animalId, ct);

        if (loteActivo is not null)
            throw new AnimalYaEnLoteActivoException(animalId, loteActivo.Id);

        var lote = await _loteRepo.ObtenerPorIdAsync(loteId, ct)
            ?? throw new DomainException($"Lote con ID '{loteId}' no encontrado.");

        lote.AgregarAnimal(animalId, fechaIngreso, MotivoMovimiento.IngresoInicial);
        _loteRepo.Actualizar(lote);
    }
}
