using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Inversion.Commands.CrearAporteSocio;

public sealed record CrearAporteSocioCommand(
    Guid SocioId,
    Guid ItemInversionId,
    decimal Monto,
    string Moneda
) : ICommand<Guid>;
