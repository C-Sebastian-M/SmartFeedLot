using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.RetirarAnimalPotrero;

public sealed class RetirarAnimalPotreroCommandHandler : IRequestHandler<RetirarAnimalPotreroCommand, Result>
{
    private readonly IPotreroRepository _repo;
    private readonly IUnitOfWork _uow;
    public RetirarAnimalPotreroCommandHandler(IPotreroRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result> Handle(RetirarAnimalPotreroCommand request, CancellationToken ct)
    {
        var potrero = await _repo.ObtenerPorIdAsync(request.PotreroId, ct);
        if (potrero is null) return Result.Failure("Potrero no encontrado.");

        potrero.RetirarAnimal(request.EstanciaId, request.FechaSalida);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
