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
        // AsNoTracking para que el nuevo ActividadManoObra no quede en el grafo tracked
        // y Add() lo marque limpio como EntityState.Added → INSERT
        var empleado = await _repo.ObtenerPorIdSinTrackingAsync(request.EmpleadoId, ct);
        if (empleado is null) return Result<Guid>.NotFound($"Empleado {request.EmpleadoId} no encontrado.");

        var costo = Dinero.Crear(request.Costo, request.Moneda);
        var actividad = empleado.RegistrarActividad(request.Tipo, request.Fecha, costo);
        _repo.AgregarActividad(actividad);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(actividad.Id);
    }
}
