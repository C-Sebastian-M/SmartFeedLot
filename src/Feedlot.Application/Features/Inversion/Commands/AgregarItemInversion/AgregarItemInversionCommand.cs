using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Inversion.Commands.AgregarItemInversion;

public sealed record AgregarItemInversionCommand(
    Guid EtapaId,
    string Producto,
    decimal Monto,
    string Moneda,
    string? Observacion,
    string Estado,
    decimal PorcentajeAvance
) : ICommand<Guid>;
