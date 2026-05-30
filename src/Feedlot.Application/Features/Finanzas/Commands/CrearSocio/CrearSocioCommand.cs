using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearSocio;

public sealed record CrearSocioCommand(
    string Nombre,
    decimal Participacion
) : ICommand<Guid>;
