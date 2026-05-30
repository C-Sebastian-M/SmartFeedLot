using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Inversion.Commands.ActualizarItemInversion;

public sealed record ActualizarItemInversionCommand(
    Guid ItemId,
    string Producto,
    decimal Monto,
    string Moneda,
    string? Observacion,
    string Estado,
    decimal PorcentajeAvance
) : ICommand;
