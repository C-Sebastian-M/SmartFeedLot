using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Nutricion.Commands.RegistrarConsumo;

public sealed record RegistrarConsumoCommand(
    Guid LoteId,
    Guid RacionId,
    DateOnly Fecha,
    decimal CantidadKg,
    decimal CostoTotal,
    string Moneda,
    Guid RegistradoPorId
) : IRequest<Result<Guid>>;
