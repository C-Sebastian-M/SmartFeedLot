using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Commands.CrearLote;

public sealed record CrearLoteCommand(
    string Codigo,
    string Nombre,
    int CapacidadMaxima
) : IRequest<Result<Guid>>;
