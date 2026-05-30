using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.RegistrarActividadManoObra;

public sealed class RegistrarActividadManoObraCommandHandler : IRequestHandler<RegistrarActividadManoObraCommand, Result<Guid>>
{
    private readonly IEmpleadoRepository _repo;
    private readonly IUnitOfWork _uow;
    public RegistrarActividadManoObraCommandHandler(IEmpleadoRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result<Guid>> Handle(RegistrarActividadManoObraCommand request, CancellationToken ct)
    {
        var empleado = await _repo.ObtenerPorIdAsync(request.EmpleadoId, ct);
        if (empleado is null) return Result<Guid>.Failure("Empleado no encontrado.");

        var costo = Dinero.Crear(request.Costo, request.Moneda);
        var actividad = empleado.RegistrarActividad(request.Tipo, request.Fecha, costo);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(actividad.Id);
    }
}
