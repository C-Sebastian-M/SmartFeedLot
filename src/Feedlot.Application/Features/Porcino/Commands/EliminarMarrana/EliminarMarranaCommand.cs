using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.EliminarMarrana;

public sealed record EliminarMarranaCommand(Guid MarranaId) : IRequest<Result>;

public sealed class EliminarMarranaCommandHandler : IRequestHandler<EliminarMarranaCommand, Result>
{
    private readonly IMarranaRepository _repo;
    private readonly IUnitOfWork _uow;

    public EliminarMarranaCommandHandler(IMarranaRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(EliminarMarranaCommand request, CancellationToken ct)
    {
        var marrana = await _repo.ObtenerPorIdAsync(request.MarranaId, ct);
        if (marrana is null)
            return Result.NotFound($"No se encontró la marrana {request.MarranaId}.");

        _repo.Eliminar(marrana);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
