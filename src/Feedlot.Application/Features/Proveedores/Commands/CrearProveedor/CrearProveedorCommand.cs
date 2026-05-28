using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Proveedores.Commands.CrearProveedor;

public sealed record CrearProveedorCommand(
    string Nombre,
    string? Contacto,
    string? Telefono,
    string? Email
) : ICommand<Guid>;
