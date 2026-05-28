using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Costos.Commands.RegistrarCostoOperativo;

public sealed record RegistrarCostoOperativoCommand(
    Guid LoteId,
    string Categoria,
    string Concepto,
    DateOnly Fecha,
    decimal Monto,
    string Moneda,
    string? Observaciones,
    Guid RegistradoPorId
) : ICommand<Guid>;
