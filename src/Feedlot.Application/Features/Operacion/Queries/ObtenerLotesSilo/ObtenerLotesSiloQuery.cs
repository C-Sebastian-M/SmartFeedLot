using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Queries.ObtenerLotesSilo;

public sealed record ObtenerLotesSiloQuery(bool? SoloDisponibles = null) : IRequest<Result<IReadOnlyList<LoteSiloDto>>>;

public sealed class LoteSiloDto
{
    public Guid Id { get; init; }
    public Guid? CorteCaniaId { get; init; }
    public DateOnly FechaProduccion { get; init; }
    public int Bolsas { get; init; }
    public decimal CostoUnitarioMonto { get; init; }
    public string CostoUnitarioMoneda { get; init; } = null!;
    public decimal CostoTotal { get; init; }
    public string? Observacion { get; init; }
}

public sealed class ObtenerLotesSiloQueryHandler : IRequestHandler<ObtenerLotesSiloQuery, Result<IReadOnlyList<LoteSiloDto>>>
{
    private readonly ILoteSiloRepository _repo;
    public ObtenerLotesSiloQueryHandler(ILoteSiloRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<LoteSiloDto>>> Handle(ObtenerLotesSiloQuery request, CancellationToken ct)
    {
        IReadOnlyList<Domain.Entities.LoteSilo> lotes;
        if (request.SoloDisponibles == true)
            lotes = await _repo.ObtenerDisponiblesAsync(ct);
        else
            lotes = await _repo.ObtenerTodosAsync(ct);

        var dtos = lotes.Select(l => new LoteSiloDto
        {
            Id = l.Id,
            CorteCaniaId = l.CorteCaniaId,
            FechaProduccion = l.FechaProduccion,
            Bolsas = l.Bolsas,
            CostoUnitarioMonto = l.CostoUnitario.Monto,
            CostoUnitarioMoneda = l.CostoUnitario.Moneda,
            CostoTotal = l.CostoTotal,
            Observacion = l.Observacion,
        }).ToList();
        return Result<IReadOnlyList<LoteSiloDto>>.Success(dtos);
    }
}
