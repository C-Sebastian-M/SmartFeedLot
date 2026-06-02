using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.EliminarCorteCania;

public sealed record EliminarCorteCaniaCommand(Guid CorteId) : ICommand;

public sealed class EliminarCorteCaniaCommandHandler : IRequestHandler<EliminarCorteCaniaCommand, Result>
{
    private readonly ICultivoCaniaRepository _repo;

    public EliminarCorteCaniaCommandHandler(ICultivoCaniaRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(EliminarCorteCaniaCommand request, CancellationToken ct)
    {
        var corte = await _repo.ObtenerCortePorIdAsync(request.CorteId, ct);
        if (corte is null)
            return Result.NotFound($"No se encontró el corte {request.CorteId}.");

        _repo.EliminarCorte(corte);
        return Result.Success();
    }
}
