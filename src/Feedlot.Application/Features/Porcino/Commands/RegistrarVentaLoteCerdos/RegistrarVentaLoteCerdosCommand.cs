using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Porcino.Commands.RegistrarVentaLoteCerdos;

public sealed record RegistrarVentaLoteCerdosCommand(
    Guid LoteId,
    DateOnly FechaVenta,
    decimal PrecioVentaKg,
    string Moneda) : ICommand;
