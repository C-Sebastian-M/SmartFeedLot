using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.EliminarLoteSilo;

public sealed record EliminarLoteSiloCommand(Guid LoteSiloId) : ICommand;

public sealed class EliminarLoteSiloCommandHandler : IRequestHandler<EliminarLoteSiloCommand, Result>
{
    private readonly ILoteSiloRepository _repo;
    private readonly IUnitOfWork _uow;

    public EliminarLoteSiloCommandHandler(ILoteSiloRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(EliminarLoteSiloCommand request, CancellationToken ct)
    {
        var lote = await _repo.ObtenerPorIdAsync(request.LoteSiloId, ct);
        if (lote is null)
            return Result.NotFound($"No se encontró el lote de silo {request.LoteSiloId}.");

        _repo.Eliminar(lote);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
