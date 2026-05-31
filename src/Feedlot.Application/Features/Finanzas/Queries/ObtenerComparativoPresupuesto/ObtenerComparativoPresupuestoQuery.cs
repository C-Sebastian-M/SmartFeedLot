using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Queries.ObtenerComparativoPresupuesto;

/// <summary>
/// Comparativo real vs presupuesto por categoría de gasto para un período (RF-052).
/// Devuelve cada categoría con: presupuestado, ejecutado real y desviación %.
/// </summary>
public sealed record ObtenerComparativoPresupuestoQuery(
    int Anio,
    int? Mes = null
) : IRequest<Result<ComparativoPresupuestoDto>>;

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class ComparativoPresupuestoDto
{
    public int Anio { get; init; }
    public int? Mes { get; init; }
    public IReadOnlyList<LineaComparativaDto> Lineas { get; init; } = [];
    public decimal TotalPresupuestado => Lineas.Sum(l => l.Presupuestado);
    public decimal TotalReal => Lineas.Sum(l => l.Real);
    public decimal TotalDesviacion => TotalReal - TotalPresupuestado;
    public decimal PorcentajeEjecucion =>
        TotalPresupuestado == 0 ? 0 : Math.Round(TotalReal / TotalPresupuestado * 100, 1);
}

public sealed class LineaComparativaDto
{
    public Guid CategoriaId { get; init; }
    public string CategoriaNombre { get; init; } = null!;
    public string CategoriaTipo { get; init; } = null!;
    public decimal Presupuestado { get; init; }
    public decimal Real { get; init; }
    public decimal Desviacion => Real - Presupuestado;
    public decimal PorcentajeEjecucion =>
        Presupuestado == 0 ? (Real > 0 ? 100 : 0) : Math.Round(Real / Presupuestado * 100, 1);
    /// <summary>Verde (&lt;90%), Amarillo (90-110%), Rojo (&gt;110%)</summary>
    public string Semaforo => PorcentajeEjecucion switch
    {
        < 90 => "verde",
        <= 110 => "amarillo",
        _ => "rojo"
    };
}

// ─── Handler ──────────────────────────────────────────────────────────────────

public sealed class ObtenerComparativoPresupuestoQueryHandler
    : IRequestHandler<ObtenerComparativoPresupuestoQuery, Result<ComparativoPresupuestoDto>>
{
    private readonly IPresupuestoRepository _presupuestoRepo;
    private readonly IMovimientoFinancieroRepository _movRepo;
    private readonly ICategoriaGastoRepository _categoriaRepo;

    public ObtenerComparativoPresupuestoQueryHandler(
        IPresupuestoRepository presupuestoRepo,
        IMovimientoFinancieroRepository movRepo,
        ICategoriaGastoRepository categoriaRepo)
    {
        _presupuestoRepo = presupuestoRepo;
        _movRepo = movRepo;
        _categoriaRepo = categoriaRepo;
    }

    public async Task<Result<ComparativoPresupuestoDto>> Handle(
        ObtenerComparativoPresupuestoQuery request, CancellationToken ct)
    {
        var presupuestos = await _presupuestoRepo.ObtenerPorPeriodoAsync(request.Anio, request.Mes, ct);
        var movimientos = await _movRepo.ObtenerPorFiltroAsync(request.Anio, request.Mes, null, null, null, ct);
        var todasCategorias = await _categoriaRepo.ObtenerTodosAsync(ct);

        // Real por categoría
        var realPorCategoria = movimientos
            .GroupBy(m => m.CategoriaGastoId)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Monto.Monto));

        // Presupuesto por categoría
        var presupuestoPorCategoria = presupuestos
            .ToDictionary(p => p.CategoriaGastoId, p => p);

        // Unión: todas las categorías que tienen presupuesto O movimientos reales
        var categoriaIds = presupuestoPorCategoria.Keys
            .Union(realPorCategoria.Keys)
            .Distinct()
            .ToList();

        var lineas = new List<LineaComparativaDto>();

        foreach (var catId in categoriaIds)
        {
            var categoria = todasCategorias.FirstOrDefault(c => c.Id == catId);
            if (categoria is null) continue;

            decimal presupuestado = presupuestoPorCategoria.TryGetValue(catId, out var p)
                ? p.MontoPresupuestado.Monto : 0;

            decimal real = realPorCategoria.GetValueOrDefault(catId);

            lineas.Add(new LineaComparativaDto
            {
                CategoriaId = catId,
                CategoriaNombre = categoria.Nombre,
                CategoriaTipo = categoria.Tipo.ToString(),
                Presupuestado = presupuestado,
                Real = real,
            });
        }

        var dto = new ComparativoPresupuestoDto
        {
            Anio = request.Anio,
            Mes = request.Mes,
            Lineas = lineas.OrderBy(l => l.CategoriaTipo).ThenBy(l => l.CategoriaNombre).ToList(),
        };

        return Result<ComparativoPresupuestoDto>.Success(dto);
    }
}
