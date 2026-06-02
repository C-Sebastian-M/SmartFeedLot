using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearEmpleado;

public sealed class CrearEmpleadoCommandHandler : IRequestHandler<CrearEmpleadoCommand, Result<Guid>>
{
    private readonly IEmpleadoRepository _repo;
    public CrearEmpleadoCommandHandler(IEmpleadoRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(CrearEmpleadoCommand request, CancellationToken ct)
    {
        var pago = Dinero.Crear(request.PagoMensual, request.Moneda);
        var empleado = Empleado.Crear(request.Nombre, pago);
        await _repo.AgregarAsync(empleado, ct);
        return Result<Guid>.Success(empleado.Id);
    }
}
