using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Queries.ObtenerFlujoCaja;

/// <summary>
/// Flujo de caja mensual para un año: ingresos, egresos, saldo neto y saldo acumulado.
/// </summary>
public sealed record ObtenerFlujoCajaQuery(
    int Anio,
    string? Origen = null
) : IRequest<Result<FlujoCajaDto>>;

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class FlujoCajaDto
{
    public int Anio { get; init; }
    public string? Origen { get; init; }
    public IReadOnlyList<FlujoCajaMesDto> Meses { get; init; } = [];
    public decimal TotalIngresos => Meses.Sum(m => m.Ingresos);
    public decimal TotalEgresos => Meses.Sum(m => m.Egresos);
    public decimal SaldoNeto => TotalIngresos - TotalEgresos;
}

public sealed class FlujoCajaMesDto
{
    public int Mes { get; init; }
    public string NombreMes { get; init; } = null!;
    public decimal Ingresos { get; init; }
    public decimal Egresos { get; init; }
    public decimal SaldoNeto => Ingresos - Egresos;
    public decimal SaldoAcumulado { get; init; }
}

// ─── Handler ──────────────────────────────────────────────────────────────────

public sealed class ObtenerFlujoCajaQueryHandler
    : IRequestHandler<ObtenerFlujoCajaQuery, Result<FlujoCajaDto>>
{
    private readonly IMovimientoFinancieroRepository _movRepo;
    private readonly IVentaRepository _ventaRepo;
    private readonly IPrestamoRepository _prestamoRepo;

    public ObtenerFlujoCajaQueryHandler(
        IMovimientoFinancieroRepository movRepo,
        IVentaRepository ventaRepo,
        IPrestamoRepository prestamoRepo)
    {
        _movRepo = movRepo;
        _ventaRepo = ventaRepo;
        _prestamoRepo = prestamoRepo;
    }

    public async Task<Result<FlujoCajaDto>> Handle(
        ObtenerFlujoCajaQuery request,
        CancellationToken ct)
    {
        OrigenFinanciero? origen = null;
        if (!string.IsNullOrWhiteSpace(request.Origen))
        {
            if (!Enum.TryParse<OrigenFinanciero>(request.Origen, ignoreCase: true, out var parsed))
                return Result<FlujoCajaDto>.Failure("Origen financiero inválido.");
            origen = parsed;
        }

        // Cargar datos del año completo
        var ventas = await _ventaRepo.ObtenerPorPeriodoAsync(request.Anio, null, ct);
        var movimientos = await _movRepo.ObtenerPorFiltroAsync(request.Anio, null, origen, null, null, ct);
        var prestamos = await _prestamoRepo.ObtenerTodosAsync(ct);

        // Agrupar ventas por mes
        var ingresosPorMes = ventas
            .GroupBy(v => v.Fecha.Month)
            .ToDictionary(g => g.Key, g => g.Sum(v => v.MontoTotal));

        // Agrupar egresos por mes (movimientos + cuotas de préstamo)
        var egresosPorMes = movimientos
            .GroupBy(m => m.PeriodoMes)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Monto.Monto));

        // Sumar cuotas (capital + interés = pago total de deuda) por mes
        foreach (var prestamo in prestamos)
        {
            foreach (var cuota in prestamo.Cuotas)
            {
                if (cuota.FechaVencimiento.Year != request.Anio) continue;
                var m = cuota.FechaVencimiento.Month;
                egresosPorMes[m] = egresosPorMes.GetValueOrDefault(m) + cuota.Cuota.Monto;
            }
        }

        var meses = new List<FlujoCajaMesDto>();
        decimal acumulado = 0;

        for (int mes = 1; mes <= 12; mes++)
        {
            decimal ingresos = ingresosPorMes.GetValueOrDefault(mes);
            decimal egresos = egresosPorMes.GetValueOrDefault(mes);
            acumulado += ingresos - egresos;

            meses.Add(new FlujoCajaMesDto
            {
                Mes = mes,
                NombreMes = NombreMes(mes),
                Ingresos = ingresos,
                Egresos = egresos,
                SaldoAcumulado = acumulado,
            });
        }

        return Result<FlujoCajaDto>.Success(new FlujoCajaDto
        {
            Anio = request.Anio,
            Origen = request.Origen,
            Meses = meses,
        });
    }

    private static string NombreMes(int mes) => mes switch
    {
        1 => "Ene", 2 => "Feb", 3 => "Mar", 4 => "Abr",
        5 => "May", 6 => "Jun", 7 => "Jul", 8 => "Ago",
        9 => "Sep", 10 => "Oct", 11 => "Nov", 12 => "Dic",
        _ => mes.ToString()
    };
}
