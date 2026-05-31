using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Queries.ObtenerMarranas;

public sealed record ObtenerMarranasQuery : IRequest<Result<IReadOnlyList<MarranaDto>>>;

public sealed class MarranaDto
{
    public Guid Id { get; init; }
    public string Identificacion { get; init; } = null!;
    public DateOnly FechaCompra { get; init; }
    public decimal CostoMonto { get; init; }
    public string CostoMoneda { get; init; } = null!;
    public List<CamadaDto> Camadas { get; init; } = new();
}

public sealed class CamadaDto
{
    public Guid Id { get; init; }
    public Guid MarranaId { get; init; }
    public DateOnly FechaNacimiento { get; init; }
    public int NLechones { get; init; }
    public string Estado { get; init; } = null!;
}

public sealed class ObtenerMarranasQueryHandler : IRequestHandler<ObtenerMarranasQuery, Result<IReadOnlyList<MarranaDto>>>
{
    private readonly IMarranaRepository _repo;
    public ObtenerMarranasQueryHandler(IMarranaRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<MarranaDto>>> Handle(ObtenerMarranasQuery request, CancellationToken ct)
    {
        var marranas = await _repo.ObtenerTodosAsync(ct);
        var dtos = marranas.Select(m => new MarranaDto
        {
            Id = m.Id,
            Identificacion = m.Identificacion,
            FechaCompra = m.FechaCompra,
            CostoMonto = m.Costo.Monto,
            CostoMoneda = m.Costo.Moneda,
            Camadas = m.Camadas.Select(c => new CamadaDto
            {
                Id = c.Id,
                MarranaId = c.MarranaId,
                FechaNacimiento = c.FechaNacimiento,
                NLechones = c.NLechones,
                Estado = c.Estado.ToString(),
            }).ToList()
        }).ToList();
        return Result<IReadOnlyList<MarranaDto>>.Success(dtos);
    }
}
