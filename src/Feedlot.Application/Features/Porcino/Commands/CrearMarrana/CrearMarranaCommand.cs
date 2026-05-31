using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Porcino.Commands.CrearMarrana;

public sealed record CrearMarranaCommand(
    string Identificacion,
    DateOnly FechaCompra,
    decimal Costo,
    string Moneda) : ICommand<Guid>;
