using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Compras.Commands.CrearCompra;

public sealed class CrearCompraCommandHandler
    : IRequestHandler<CrearCompraCommand, Result<Guid>>
{
    private readonly ICompraRepository _compraRepository;

    public CrearCompraCommandHandler(ICompraRepository compraRepository)
    {
        _compraRepository = compraRepository;
    }

    public async Task<Result<Guid>> Handle(CrearCompraCommand request, CancellationToken ct)
    {
        Compra compra;
        if (request.TipoCompra == "Ganado")
        {
            if (!request.CantidadCabezas.HasValue || !request.PrecioPorCabeza.HasValue ||
                !request.PesoPromedioKg.HasValue || !request.LoteId.HasValue)
                return Result<Guid>.Validation("Para compras de ganado se requieren: cantidadCabezas, precioPorCabeza, pesoPromedioKg, loteId.");

            compra = Compra.CrearCompraGanado(
                request.ProveedorId, request.Fecha,
                request.CantidadCabezas.Value, request.PrecioPorCabeza.Value,
                request.PesoPromedioKg.Value, request.LoteId.Value,
                request.CostoTotal, request.Moneda, request.Descripcion);
        }
        else if (request.TipoCompra == "Insumo")
        {
            if (string.IsNullOrWhiteSpace(request.TipoInsumo) || !request.CantidadInsumo.HasValue ||
                string.IsNullOrWhiteSpace(request.UnidadMedida))
                return Result<Guid>.Validation("Para compras de insumo se requieren: tipoInsumo, cantidadInsumo, unidadMedida.");

            compra = Compra.CrearCompraInsumo(
                request.ProveedorId, request.Fecha,
                request.TipoInsumo, request.CantidadInsumo.Value,
                request.UnidadMedida, request.CostoTotal, request.Moneda,
                request.Descripcion);
        }
        else
        {
            return Result<Guid>.Validation("El tipo de compra debe ser 'Ganado' o 'Insumo'.");
        }

        await _compraRepository.AgregarAsync(compra, ct);
        return Result<Guid>.Success(compra.Id);
    }
}
