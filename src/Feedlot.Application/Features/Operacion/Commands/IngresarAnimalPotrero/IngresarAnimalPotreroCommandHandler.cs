using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.IngresarAnimalPotrero;

public sealed class IngresarAnimalPotreroCommandHandler : IRequestHandler<IngresarAnimalPotreroCommand, Result<Guid>>
{
    private readonly IPotreroRepository _repo;
    private readonly IUnitOfWork _uow;
    public IngresarAnimalPotreroCommandHandler(IPotreroRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result<Guid>> Handle(IngresarAnimalPotreroCommand request, CancellationToken ct)
    {
        var potrero = await _repo.ObtenerPorIdAsync(request.PotreroId, ct);
        if (potrero is null) return Result<Guid>.Failure("Potrero no encontrado.");

        var estancia = potrero.IngresarAnimal(request.AnimalId, request.FechaEntrada);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(estancia.Id);
    }
}
