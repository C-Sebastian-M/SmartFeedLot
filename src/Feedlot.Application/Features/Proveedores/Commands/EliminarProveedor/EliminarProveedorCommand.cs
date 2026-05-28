using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Proveedores.Commands.EliminarProveedor;

public sealed record EliminarProveedorCommand(Guid Id) : ICommand;
