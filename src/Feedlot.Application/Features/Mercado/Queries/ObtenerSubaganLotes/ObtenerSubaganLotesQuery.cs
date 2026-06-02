using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Queries.ObtenerSubaganLotes;

public sealed record ObtenerSubaganLotesQuery(Guid EventoId)
    : IRequest<Result<IReadOnlyList<SubaganLoteDto>>>;

public sealed record SubaganLoteDto(
    Guid Id,
    int LoteId,
    int NumeroLote,
    string CodigoTipo,
    string DescripcionTipo,
    int Cantidad,
    decimal PesoTotal,
    decimal PesoProm,
    decimal PrecioPorKg,
    string Procedencia,
    string? Observaciones,
    DateOnly Fecha);

public sealed class ObtenerSubaganLotesQueryHandler
    : IRequestHandler<ObtenerSubaganLotesQuery, Result<IReadOnlyList<SubaganLoteDto>>>
{
    private readonly ISubaganEventoRepository _repo;
    public ObtenerSubaganLotesQueryHandler(ISubaganEventoRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<SubaganLoteDto>>> Handle(
        ObtenerSubaganLotesQuery request, CancellationToken ct)
    {
        var lotes = await _repo.ObtenerLotesPorEventoAsync(request.EventoId, ct);

        var dtos = lotes.Select(l => new SubaganLoteDto(
            l.Id, l.LoteId, l.NumeroLote, l.CodigoTipo, l.DescripcionTipo,
            l.Cantidad, l.PesoTotal, l.PesoProm, l.PrecioPorKg,
            l.Procedencia, l.Observaciones, l.Fecha
        )).ToList().AsReadOnly();

        return Result<IReadOnlyList<SubaganLoteDto>>.Success(dtos);
    }
}
