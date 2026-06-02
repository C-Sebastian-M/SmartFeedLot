using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.EliminarPotrero;

public sealed record EliminarPotreroCommand(Guid PotreroId) : ICommand;

public sealed class EliminarPotreroCommandHandler : IRequestHandler<EliminarPotreroCommand, Result>
{
    private readonly IPotreroRepository _repo;
    private readonly IUnitOfWork _uow;

    public EliminarPotreroCommandHandler(IPotreroRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(EliminarPotreroCommand request, CancellationToken ct)
    {
        var potrero = await _repo.ObtenerPorIdAsync(request.PotreroId, ct);
        if (potrero is null)
            return Result.NotFound($"No se encontró el potrero {request.PotreroId}.");

        _repo.Eliminar(potrero);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
