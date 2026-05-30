using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearEmpleado;

public sealed class CrearEmpleadoCommandHandler : IRequestHandler<CrearEmpleadoCommand, Result<Guid>>
{
    private readonly IEmpleadoRepository _repo;
    private readonly IUnitOfWork _uow;
    public CrearEmpleadoCommandHandler(IEmpleadoRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result<Guid>> Handle(CrearEmpleadoCommand request, CancellationToken ct)
    {
        var pago = Dinero.Crear(request.PagoMensual, request.Moneda);
        var empleado = Empleado.Crear(request.Nombre, pago);
        await _repo.AgregarAsync(empleado, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(empleado.Id);
    }
}
