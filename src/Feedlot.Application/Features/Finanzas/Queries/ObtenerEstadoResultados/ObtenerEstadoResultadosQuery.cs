using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Queries.ObtenerEstadoResultados;

/// <summary>
/// Estado de Resultados (P&amp;L) para un año completo o un mes concreto.
/// Ingresos: ventas del período.
/// Egresos: MovimientosFinancieros agrupados por TipoCategoriaGasto.
/// Servicio deuda: intereses de cuotas vencidas en el período.
/// </summary>
public sealed record ObtenerEstadoResultadosQuery(
    int Anio,
    int? Mes = null,
    string? Origen = null
) : IRequest<Result<EstadoResultadosDto>>;

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class EstadoResultadosDto
{
    public int Anio { get; init; }
    public int? Mes { get; init; }
    public string? Origen { get; init; }
    public string Moneda { get; init; } = "COP";

    public decimal TotalIngresos { get; init; }
    public IReadOnlyList<LineaResultadoDto> Ingresos { get; init; } = [];

    public decimal TotalCostosDirectos { get; init; }
    public IReadOnlyList<LineaResultadoDto> CostosDirectos { get; init; } = [];

    public decimal UtilidadBruta => TotalIngresos - TotalCostosDirectos;

    public decimal TotalGastosIndirectos { get; init; }
    public IReadOnlyList<LineaResultadoDto> GastosIndirectos { get; init; } = [];

    public decimal TotalGastosOperativos { get; init; }
    public IReadOnlyList<LineaResultadoDto> GastosOperativos { get; init; } = [];

    public decimal TotalInteresesPrestamo { get; init; }

    public decimal UtilidadOperativa =>
        UtilidadBruta - TotalGastosIndirectos - TotalGastosOperativos - TotalInteresesPrestamo;

    public decimal TotalInversiones { get; init; }
    public IReadOnlyList<LineaResultadoDto> Inversiones { get; init; } = [];

    public decimal UtilidadNeta => UtilidadOperativa - TotalInversiones;
}

public sealed class LineaResultadoDto
{
    public string Concepto { get; init; } = null!;
    public decimal Monto { get; init; }
}

// ─── Handler ──────────────────────────────────────────────────────────────────

public sealed class ObtenerEstadoResultadosQueryHandler
    : IRequestHandler<ObtenerEstadoResultadosQuery, Result<EstadoResultadosDto>>
{
    private readonly IMovimientoFinancieroRepository _movRepo;
    private readonly IVentaRepository _ventaRepo;
    private readonly IPrestamoRepository _prestamoRepo;

    public ObtenerEstadoResultadosQueryHandler(
        IMovimientoFinancieroRepository movRepo,
        IVentaRepository ventaRepo,
        IPrestamoRepository prestamoRepo)
    {
        _movRepo = movRepo;
        _ventaRepo = ventaRepo;
        _prestamoRepo = prestamoRepo;
    }

    public async Task<Result<EstadoResultadosDto>> Handle(
        ObtenerEstadoResultadosQuery request,
        CancellationToken ct)
    {
        OrigenFinanciero? origen = null;
        if (!string.IsNullOrWhiteSpace(request.Origen))
        {
            if (!Enum.TryParse<OrigenFinanciero>(request.Origen, ignoreCase: true, out var parsed))
                return Result<EstadoResultadosDto>.Failure("Origen financiero inválido.");
            origen = parsed;
        }

        // ── Ingresos: ventas del período ──────────────────────────────────────
        var ventas = await _ventaRepo.ObtenerPorPeriodoAsync(request.Anio, request.Mes, ct);
        var totalIngresos = ventas.Sum(v => v.MontoTotal);
        var lineasIngresos = ventas
            .GroupBy(v => v.Fecha.Month)
            .Select(g => new LineaResultadoDto
            {
                Concepto = request.Mes.HasValue
                    ? "Ingresos por ventas"
                    : $"Ventas {NombreMes(g.Key)}",
                Monto = g.Sum(v => v.MontoTotal)
            })
            .ToList();

        if (!lineasIngresos.Any() && ventas.Any())
            lineasIngresos = [new LineaResultadoDto { Concepto = "Ingresos por ventas", Monto = totalIngresos }];

        // ── Egresos: movimientos agrupados por categoría ──────────────────────
        var movimientos = await _movRepo.ObtenerPorFiltroAsync(
            request.Anio, request.Mes, origen, null, null, ct);

        var porTipo = movimientos
            .GroupBy(m => m.CategoriaGasto.Tipo)
            .ToDictionary(g => g.Key, g => g.ToList());

        LineasPorCategoria(porTipo, TipoCategoriaGasto.Directo,
            out var directos, out var totalDirectos);
        LineasPorCategoria(porTipo, TipoCategoriaGasto.Indirecto,
            out var indirectos, out var totalIndirectos);
        LineasPorCategoria(porTipo, TipoCategoriaGasto.Operativo,
            out var operativos, out var totalOperativos);
        LineasPorCategoria(porTipo, TipoCategoriaGasto.Inversion,
            out var inversiones, out var totalInversiones);

        // ── Intereses de préstamos vencidos en el período ─────────────────────
        var prestamos = await _prestamoRepo.ObtenerTodosAsync(ct);
        decimal totalIntereses = 0;
        foreach (var prestamo in prestamos)
        {
            foreach (var cuota in prestamo.Cuotas)
            {
                if (cuota.FechaVencimiento.Year != request.Anio) continue;
                if (request.Mes.HasValue && cuota.FechaVencimiento.Month != request.Mes.Value) continue;
                totalIntereses += cuota.Interes.Monto;
            }
        }

        var dto = new EstadoResultadosDto
        {
            Anio = request.Anio,
            Mes = request.Mes,
            Origen = request.Origen,
            TotalIngresos = totalIngresos,
            Ingresos = lineasIngresos,
            TotalCostosDirectos = totalDirectos,
            CostosDirectos = directos,
            TotalGastosIndirectos = totalIndirectos,
            GastosIndirectos = indirectos,
            TotalGastosOperativos = totalOperativos,
            GastosOperativos = operativos,
            TotalInteresesPrestamo = totalIntereses,
            TotalInversiones = totalInversiones,
            Inversiones = inversiones,
        };

        return Result<EstadoResultadosDto>.Success(dto);
    }

    private static void LineasPorCategoria(
        Dictionary<TipoCategoriaGasto, List<MovimientoFinanciero>> porTipo,
        TipoCategoriaGasto tipo,
        out List<LineaResultadoDto> lineas,
        out decimal total)
    {
        if (!porTipo.TryGetValue(tipo, out var movs))
        {
            lineas = [];
            total = 0;
            return;
        }

        lineas = movs
            .GroupBy(m => m.CategoriaGasto.Nombre)
            .Select(g => new LineaResultadoDto
            {
                Concepto = g.Key,
                Monto = g.Sum(m => m.Monto.Monto)
            })
            .OrderByDescending(l => l.Monto)
            .ToList();

        total = lineas.Sum(l => l.Monto);
    }

    private static string NombreMes(int mes) => mes switch
    {
        1 => "Enero", 2 => "Febrero", 3 => "Marzo", 4 => "Abril",
        5 => "Mayo", 6 => "Junio", 7 => "Julio", 8 => "Agosto",
        9 => "Septiembre", 10 => "Octubre", 11 => "Noviembre", 12 => "Diciembre",
        _ => mes.ToString()
    };
}
