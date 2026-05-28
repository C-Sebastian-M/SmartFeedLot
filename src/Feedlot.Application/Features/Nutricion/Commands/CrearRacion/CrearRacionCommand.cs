using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Nutricion.Commands.CrearRacion;

public sealed record CrearRacionCommand(
    string Nombre,
    decimal CostoKg,
    string Moneda,
    decimal ProteinaPct,
    decimal EnergiaMcal
) : ICommand<Guid>;
