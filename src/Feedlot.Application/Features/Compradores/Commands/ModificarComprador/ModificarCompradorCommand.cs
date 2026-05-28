using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Compradores.Commands.ModificarComprador;

public sealed record ModificarCompradorCommand(
    Guid Id,
    string Nombre,
    string? Contacto,
    string? Telefono,
    string? Email
) : ICommand;
