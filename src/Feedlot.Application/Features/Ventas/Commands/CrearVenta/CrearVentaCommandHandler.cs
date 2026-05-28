using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Ventas.Commands.CrearVenta;

public sealed class CrearVentaCommandHandler
    : IRequestHandler<CrearVentaCommand, Result<Guid>>
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly ILoteRepository _loteRepository;

    public CrearVentaCommandHandler(
        IVentaRepository ventaRepository,
        IAnimalRepository animalRepository,
        ILoteRepository loteRepository)
    {
        _ventaRepository = ventaRepository;
        _animalRepository = animalRepository;
        _loteRepository = loteRepository;
    }

    public async Task<Result<Guid>> Handle(CrearVentaCommand request, CancellationToken ct)
    {
        var venta = Venta.Crear(request.CompradorId, request.Fecha, request.Moneda, request.Descripcion);

        foreach (var animalInput in request.Animales)
        {
            var animal = await _animalRepository.ObtenerPorIdAsync(animalInput.AnimalId, ct);
            if (animal is null)
                return Result<Guid>.NotFound($"Animal {animalInput.AnimalId} no encontrado.");

            if (!animal.EstaActivo)
                return Result<Guid>.Validation($"El animal {animal.CodigoIdentificacion.Valor} no está activo (estado: {animal.EstadoProductivo}).");

            venta.AgregarItem(animalInput.AnimalId, animalInput.PrecioVenta, animalInput.PesoVentaKg);

            animal.MarcarComoVendido();

            var loteActivo = await _loteRepository.ObtenerLoteActivoDelAnimalAsync(animalInput.AnimalId, ct);
            if (loteActivo is not null)
            {
                loteActivo.RetirarAnimal(animalInput.AnimalId, request.Fecha, MotivoMovimiento.Venta);
                _loteRepository.Actualizar(loteActivo);
            }
        }

        await _ventaRepository.AgregarAsync(venta, ct);
        return Result<Guid>.Success(venta.Id);
    }
}
