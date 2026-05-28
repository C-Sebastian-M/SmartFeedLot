using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Proveedores.Commands.CrearProveedor;

public sealed class CrearProveedorCommandHandler
    : IRequestHandler<CrearProveedorCommand, Result<Guid>>
{
    private readonly IProveedorRepository _proveedorRepository;

    public CrearProveedorCommandHandler(IProveedorRepository proveedorRepository)
    {
        _proveedorRepository = proveedorRepository;
    }

    public async Task<Result<Guid>> Handle(CrearProveedorCommand request, CancellationToken ct)
    {
        var proveedor = Proveedor.Crear(request.Nombre, request.Contacto, request.Telefono, request.Email);
        await _proveedorRepository.AgregarAsync(proveedor, ct);
        return Result<Guid>.Success(proveedor.Id);
    }
}
