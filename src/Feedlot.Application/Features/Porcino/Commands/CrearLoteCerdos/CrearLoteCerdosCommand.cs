using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Porcino.Commands.CrearLoteCerdos;

public sealed record CrearLoteCerdosCommand(
    string Codigo,
    DateOnly FechaInicio,
    int NAnimales,
    decimal PesoPromedioKg,
    string Ciclo,
    Guid? CamadaId = null,
    decimal? PrecioVentaKg = null,
    string? Moneda = null) : ICommand<Guid>;
