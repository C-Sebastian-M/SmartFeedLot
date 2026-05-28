using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Proveedores.Commands.ModificarProveedor;

public sealed record ModificarProveedorCommand(
    Guid Id,
    string Nombre,
    string? Contacto,
    string? Telefono,
    string? Email
) : ICommand;
