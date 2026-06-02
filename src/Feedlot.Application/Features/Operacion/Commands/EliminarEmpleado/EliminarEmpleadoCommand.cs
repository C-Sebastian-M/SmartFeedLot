using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.EliminarEmpleado;

public sealed record EliminarEmpleadoCommand(Guid EmpleadoId) : ICommand;

public sealed class EliminarEmpleadoCommandHandler : IRequestHandler<EliminarEmpleadoCommand, Result>
{
    private readonly IEmpleadoRepository _repo;
    private readonly IUnitOfWork _uow;

    public EliminarEmpleadoCommandHandler(IEmpleadoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(EliminarEmpleadoCommand request, CancellationToken ct)
    {
        var empleado = await _repo.ObtenerPorIdAsync(request.EmpleadoId, ct);
        if (empleado is null)
            return Result.NotFound($"No se encontró el empleado {request.EmpleadoId}.");

        _repo.Eliminar(empleado);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
