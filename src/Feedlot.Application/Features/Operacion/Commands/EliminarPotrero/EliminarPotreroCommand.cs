using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.EliminarPotrero;

public sealed record EliminarPotreroCommand(Guid PotreroId) : ICommand;

public sealed class EliminarPotreroCommandHandler : IRequestHandler<EliminarPotreroCommand, Result>
{
    private readonly IPotreroRepository _repo;

    public EliminarPotreroCommandHandler(IPotreroRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(EliminarPotreroCommand request, CancellationToken ct)
    {
        var potrero = await _repo.ObtenerPorIdAsync(request.PotreroId, ct);
        if (potrero is null)
            return Result.NotFound($"No se encontró el potrero {request.PotreroId}.");

        _repo.Eliminar(potrero);
        return Result.Success();
    }
}
