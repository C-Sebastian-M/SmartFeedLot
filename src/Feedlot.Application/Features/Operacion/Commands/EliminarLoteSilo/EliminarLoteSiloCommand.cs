using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.EliminarLoteSilo;

public sealed record EliminarLoteSiloCommand(Guid LoteSiloId) : ICommand;

public sealed class EliminarLoteSiloCommandHandler : IRequestHandler<EliminarLoteSiloCommand, Result>
{
    private readonly ILoteSiloRepository _repo;

    public EliminarLoteSiloCommandHandler(ILoteSiloRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(EliminarLoteSiloCommand request, CancellationToken ct)
    {
        var lote = await _repo.ObtenerPorIdAsync(request.LoteSiloId, ct);
        if (lote is null)
            return Result.NotFound($"No se encontró el lote de silo {request.LoteSiloId}.");

        _repo.Eliminar(lote);
        return Result.Success();
    }
}
