using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.EliminarCultivoCania;

public sealed record EliminarCultivoCaniaCommand(Guid CultivoCaniaId) : ICommand;

public sealed class EliminarCultivoCaniaCommandHandler : IRequestHandler<EliminarCultivoCaniaCommand, Result>
{
    private readonly ICultivoCaniaRepository _repo;

    public EliminarCultivoCaniaCommandHandler(ICultivoCaniaRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(EliminarCultivoCaniaCommand request, CancellationToken ct)
    {
        var cultivo = await _repo.ObtenerPorIdAsync(request.CultivoCaniaId, ct);
        if (cultivo is null)
            return Result.NotFound($"No se encontró el cultivo {request.CultivoCaniaId}.");

        _repo.Eliminar(cultivo);
        return Result.Success();
    }
}
