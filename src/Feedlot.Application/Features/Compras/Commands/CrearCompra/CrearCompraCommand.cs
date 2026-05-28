using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Compras.Commands.CrearCompra;

public sealed record CrearCompraCommand(
    Guid ProveedorId,
    DateOnly Fecha,
    string TipoCompra,
    decimal CostoTotal,
    string Moneda,
    string? Descripcion,
    int? CantidadCabezas,
    decimal? PrecioPorCabeza,
    decimal? PesoPromedioKg,
    Guid? LoteId,
    string? TipoInsumo,
    decimal? CantidadInsumo,
    string? UnidadMedida
) : ICommand<Guid>;
