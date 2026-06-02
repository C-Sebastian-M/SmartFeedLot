using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.EliminarEmpleado;

public sealed record EliminarEmpleadoCommand(Guid EmpleadoId) : ICommand;

public sealed class EliminarEmpleadoCommandHandler : IRequestHandler<EliminarEmpleadoCommand, Result>
{
    private readonly IEmpleadoRepository _repo;

    public EliminarEmpleadoCommandHandler(IEmpleadoRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(EliminarEmpleadoCommand request, CancellationToken ct)
    {
        var empleado = await _repo.ObtenerPorIdAsync(request.EmpleadoId, ct);
        if (empleado is null)
            return Result.NotFound($"No se encontró el empleado {request.EmpleadoId}.");

        _repo.Eliminar(empleado);
        return Result.Success();
    }
}
