using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Proveedores.Commands.ModificarProveedor;

public sealed class ModificarProveedorCommandHandler
    : IRequestHandler<ModificarProveedorCommand, Result>
{
    private readonly IProveedorRepository _proveedorRepository;

    public ModificarProveedorCommandHandler(IProveedorRepository proveedorRepository)
    {
        _proveedorRepository = proveedorRepository;
    }

    public async Task<Result> Handle(ModificarProveedorCommand request, CancellationToken ct)
    {
        var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.Id, ct);
        if (proveedor is null)
            return Result.NotFound($"Proveedor {request.Id} no encontrado.");

        proveedor.Modificar(request.Nombre, request.Contacto, request.Telefono, request.Email);
        _proveedorRepository.Actualizar(proveedor);
        return Result.Success();
    }
}
