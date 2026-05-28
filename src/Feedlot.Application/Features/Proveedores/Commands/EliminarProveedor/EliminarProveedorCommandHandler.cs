using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Proveedores.Commands.EliminarProveedor;

public sealed class EliminarProveedorCommandHandler
    : IRequestHandler<EliminarProveedorCommand, Result>
{
    private readonly IProveedorRepository _proveedorRepository;

    public EliminarProveedorCommandHandler(IProveedorRepository proveedorRepository)
    {
        _proveedorRepository = proveedorRepository;
    }

    public async Task<Result> Handle(EliminarProveedorCommand request, CancellationToken ct)
    {
        var eliminado = await _proveedorRepository.EliminarAsync(request.Id, ct);
        if (!eliminado)
            return Result.NotFound($"Proveedor {request.Id} no encontrado.");
        return Result.Success();
    }
}
