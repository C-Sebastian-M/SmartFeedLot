using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Nutricion.Commands.CrearRacion;

public sealed record CrearRacionCommand(
    string Nombre,
    decimal CostoKg,
    string Moneda,
    decimal ProteinaPct,
    decimal EnergiaMcal
) : IRequest<Result<Guid>>;
