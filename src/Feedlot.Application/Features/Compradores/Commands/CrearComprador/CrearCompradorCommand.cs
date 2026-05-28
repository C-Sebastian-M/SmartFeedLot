using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Compradores.Commands.CrearComprador;

public sealed record CrearCompradorCommand(
    string Nombre,
    string? Contacto,
    string? Telefono,
    string? Email
) : ICommand<Guid>;
