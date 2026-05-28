using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Lotes.Commands.CrearLote;

public sealed record CrearLoteCommand(
    string Nombre,
    int CapacidadMaxima
) : ICommand<Guid>;
