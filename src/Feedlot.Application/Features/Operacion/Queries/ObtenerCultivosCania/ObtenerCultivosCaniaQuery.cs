using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Queries.ObtenerCultivosCania;

public sealed record ObtenerCultivosCaniaQuery : IRequest<Result<IReadOnlyList<CultivoCaniaDto>>>;

public sealed class CorteDto
{
    public Guid Id { get; init; }
    public DateOnly Fecha { get; init; }
    public int NCalles { get; init; }
    public decimal Horas { get; init; }
    public int BolsasSilo { get; init; }
    public decimal Melaza { get; init; }
    public decimal CostoJornalMonto { get; init; }
    public string CostoJornalMoneda { get; init; } = null!;
}

public sealed class CultivoCaniaDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = null!;
    public int CallesTotales { get; init; }
    public int TotalBolsasSilo { get; init; }
    public decimal TotalHoras { get; init; }
    public int TotalCortes { get; init; }
    public List<CorteDto> Cortes { get; init; } = new();
}

public sealed class ObtenerCultivosCaniaQueryHandler : IRequestHandler<ObtenerCultivosCaniaQuery, Result<IReadOnlyList<CultivoCaniaDto>>>
{
    private readonly ICultivoCaniaRepository _repo;
    public ObtenerCultivosCaniaQueryHandler(ICultivoCaniaRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<CultivoCaniaDto>>> Handle(ObtenerCultivosCaniaQuery request, CancellationToken ct)
    {
        var cultivos = await _repo.ObtenerTodosAsync(ct);
        var dtos = cultivos.Select(c => new CultivoCaniaDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            CallesTotales = c.CallesTotales,
            TotalBolsasSilo = c.TotalBolsasSilo,
            TotalHoras = c.TotalHoras,
            TotalCortes = c.Cortes.Count,
            Cortes = c.Cortes.Select(cc => new CorteDto
            {
                Id = cc.Id, Fecha = cc.Fecha, NCalles = cc.NCalles, Horas = cc.Horas,
                BolsasSilo = cc.BolsasSilo, Melaza = cc.Melaza,
                CostoJornalMonto = cc.CostoJornal.Monto, CostoJornalMoneda = cc.CostoJornal.Moneda,
            }).ToList()
        }).ToList();
        return Result<IReadOnlyList<CultivoCaniaDto>>.Success(dtos);
    }
}
