using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

/// <summary>
/// Contrato del repositorio de Lote.
/// Incluye consulta de animales activos para validar la invariante
/// de "un animal no puede estar en dos lotes activos simultáneamente".
/// </summary>
public interface ILoteRepository
{
    Task<Lote?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Lote?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Lote>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Lote>> ObtenerActivosAsync(CancellationToken ct = default);
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default);

    /// <summary>
    /// Retorna el lote activo al que pertenece el animal, o null si no está en ninguno.
    /// Crítico para validar la invariante de pertenencia única.
    /// </summary>
    Task<Lote?> ObtenerLoteActivoDelAnimalAsync(Guid animalId, CancellationToken ct = default);

    Task AgregarAsync(Lote lote, CancellationToken ct = default);
    void Actualizar(Lote lote);
}
