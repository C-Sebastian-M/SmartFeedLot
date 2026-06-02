using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.EliminarMarrana;

public sealed record EliminarMarranaCommand(Guid MarranaId) : ICommand;

public sealed class EliminarMarranaCommandHandler : IRequestHandler<EliminarMarranaCommand, Result>
{
    private readonly IMarranaRepository _repo;

    public EliminarMarranaCommandHandler(IMarranaRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(EliminarMarranaCommand request, CancellationToken ct)
    {
        var marrana = await _repo.ObtenerPorIdAsync(request.MarranaId, ct);
        if (marrana is null)
            return Result.NotFound($"No se encontró la marrana {request.MarranaId}.");

        _repo.Eliminar(marrana);
        return Result.Success();
    }
}
