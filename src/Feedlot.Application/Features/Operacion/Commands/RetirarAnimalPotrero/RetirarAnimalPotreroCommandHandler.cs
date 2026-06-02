using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.RetirarAnimalPotrero;

public sealed class RetirarAnimalPotreroCommandHandler : IRequestHandler<RetirarAnimalPotreroCommand, Result>
{
    private readonly IPotreroRepository _repo;

    public RetirarAnimalPotreroCommandHandler(IPotreroRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(RetirarAnimalPotreroCommand request, CancellationToken ct)
    {
        var potrero = await _repo.ObtenerPorIdAsync(request.PotreroId, ct);
        if (potrero is null)
            return Result.NotFound($"No se encontró el potrero {request.PotreroId}.");

        try
        {
            potrero.RetirarAnimal(request.EstanciaId, request.FechaSalida);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
