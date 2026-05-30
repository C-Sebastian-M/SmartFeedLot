using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Operacion.Commands.CrearLoteSilo;

public sealed record CrearLoteSiloCommand(
    DateOnly FechaProduccion, int Bolsas, decimal CostoUnitario,
    string Moneda, string? Observacion, Guid? CorteCaniaId = null) : ICommand<Guid>;
