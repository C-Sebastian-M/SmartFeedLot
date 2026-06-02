using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Queries.ObtenerSubaganEventos;

public sealed record ObtenerSubaganEventosQuery : IRequest<Result<IReadOnlyList<SubaganEventoDto>>>;

public sealed record SubaganEventoDto(
    Guid Id,
    int SubaganEventoId,
    int? NumeroSubasta,
    DateOnly Fecha,
    string Sede,
    int TotalLotes,
    DateTime ImportadoEn);

public sealed class ObtenerSubaganEventosQueryHandler
    : IRequestHandler<ObtenerSubaganEventosQuery, Result<IReadOnlyList<SubaganEventoDto>>>
{
    private readonly ISubaganEventoRepository _repo;
    public ObtenerSubaganEventosQueryHandler(ISubaganEventoRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<SubaganEventoDto>>> Handle(
        ObtenerSubaganEventosQuery request, CancellationToken ct)
    {
        var eventos = await _repo.ObtenerTodosAsync(ct);

        var dtos = eventos.Select(e => new SubaganEventoDto(
            e.Id,
            e.SubaganEventoId,
            e.NumeroSubasta,
            e.Fecha,
            e.Sede,
            e.Lotes.Count,
            e.ImportadoEn
        )).ToList().AsReadOnly();

        return Result<IReadOnlyList<SubaganEventoDto>>.Success(dtos);
    }
}
