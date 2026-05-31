using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Queries.ObtenerLotesCerdos;

public sealed record ObtenerLotesCerdosQuery : IRequest<Result<IReadOnlyList<LoteCerdosDto>>>;

public sealed class LoteCerdosDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = null!;
    public DateOnly FechaInicio { get; init; }
    public int NAnimales { get; init; }
    public decimal PesoPromedioKg { get; init; }
    public string Ciclo { get; init; } = null!;
    public Guid? CamadaId { get; init; }
    public decimal? PrecioVentaKgMonto { get; init; }
    public string? PrecioVentaKgMoneda { get; init; }
    public DateOnly? FechaVenta { get; init; }
    public bool Vendido { get; init; }
}

public sealed class ObtenerLotesCerdosQueryHandler : IRequestHandler<ObtenerLotesCerdosQuery, Result<IReadOnlyList<LoteCerdosDto>>>
{
    private readonly ILoteCerdosRepository _repo;
    public ObtenerLotesCerdosQueryHandler(ILoteCerdosRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<LoteCerdosDto>>> Handle(ObtenerLotesCerdosQuery request, CancellationToken ct)
    {
        var lotes = await _repo.ObtenerTodosAsync(ct);
        var dtos = lotes.Select(l => new LoteCerdosDto
        {
            Id = l.Id,
            Codigo = l.Codigo,
            FechaInicio = l.FechaInicio,
            NAnimales = l.NAnimales,
            PesoPromedioKg = l.PesoPromedioKg,
            Ciclo = l.Ciclo,
            CamadaId = l.CamadaId,
            PrecioVentaKgMonto = l.PrecioVentaKg?.Monto,
            PrecioVentaKgMoneda = l.PrecioVentaKg?.Moneda,
            FechaVenta = l.FechaVenta,
            Vendido = l.Vendido,
        }).ToList();
        return Result<IReadOnlyList<LoteCerdosDto>>.Success(dtos);
    }
}
