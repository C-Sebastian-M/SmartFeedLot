using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.EliminarCorteCania;

public sealed record EliminarCorteCaniaCommand(Guid CorteId) : ICommand;

public sealed class EliminarCorteCaniaCommandHandler : IRequestHandler<EliminarCorteCaniaCommand, Result>
{
    private readonly ICultivoCaniaRepository _repo;
    private readonly IUnitOfWork _uow;

    public EliminarCorteCaniaCommandHandler(ICultivoCaniaRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(EliminarCorteCaniaCommand request, CancellationToken ct)
    {
        var corte = await _repo.ObtenerCortePorIdAsync(request.CorteId, ct);
        if (corte is null)
            return Result.NotFound($"No se encontró el corte {request.CorteId}.");

        _repo.EliminarCorte(corte);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
