using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Commands.EliminarSubaganEvento;

/// <summary>
/// Elimina un evento de SUBAGAN importado y, en cascada, todos sus lotes.
/// El Id es el identificador interno (Guid) del evento, no el SubaganEventoId externo.
/// </summary>
public sealed record EliminarSubaganEventoCommand(Guid Id) : ICommand;

public sealed class EliminarSubaganEventoCommandHandler
    : IRequestHandler<EliminarSubaganEventoCommand, Result>
{
    private readonly ISubaganEventoRepository _repo;

    public EliminarSubaganEventoCommandHandler(ISubaganEventoRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(EliminarSubaganEventoCommand request, CancellationToken ct)
    {
        var evento = await _repo.ObtenerPorIdAsync(request.Id, ct);
        if (evento is null)
            return Result.NotFound($"No se encontró el evento de SUBAGAN {request.Id}.");

        // Los lotes se eliminan en cascada al cargarse con el agregado (Include _lotes).
        _repo.Eliminar(evento);
        return Result.Success();
    }
}
