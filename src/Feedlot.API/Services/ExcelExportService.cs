using ClosedXML.Excel;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerComparativoPresupuesto;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerEstadoResultados;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerFlujoCaja;

namespace Feedlot.API.Services;

/// <summary>
/// Genera archivos Excel (.xlsx) para los reportes financieros.
/// Se ubica en la capa API para mantener Application libre de dependencias de presentación.
/// </summary>
public static class ExcelExportService
{
    // ── Estado de Resultados ──────────────────────────────────────────────────

    public static byte[] GenerarEstadoResultados(EstadoResultadosDto dto)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Estado de Resultados");

        var periodo = dto.Mes.HasValue
            ? $"{NombreMes(dto.Mes.Value)} {dto.Anio}"
            : $"Año {dto.Anio}";
        var origen = string.IsNullOrWhiteSpace(dto.Origen) ? "Todos los orígenes" : dto.Origen;

        // Título
        ws.Cell("A1").Value = "SmartFeedLot — Estado de Resultados";
        ws.Cell("A2").Value = $"Período: {periodo}  |  Origen: {origen}";
        ws.Cell("A3").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Range("A1:C1").Merge().Style.Font.Bold = true;
        ws.Range("A1:C1").Style.Font.FontSize = 14;

        int row = 5;

        // ── Ingresos ──────────────────────────────────────────────────────────
        row = EscribirSeccion(ws, row, "INGRESOS", dto.Ingresos, isIngreso: true);
        row = EscribirTotalPrincipal(ws, row, "Total Ingresos", dto.TotalIngresos);
        row++;

        // ── Costos Directos ───────────────────────────────────────────────────
        row = EscribirSeccion(ws, row, "(-) COSTOS DIRECTOS", dto.CostosDirectos);
        row = EscribirTotalPrincipal(ws, row, "Total Costos Directos", dto.TotalCostosDirectos);
        row++;

        // ── Utilidad Bruta ────────────────────────────────────────────────────
        row = EscribirSubtotal(ws, row, "UTILIDAD BRUTA", dto.UtilidadBruta);
        row++;

        // ── Gastos Indirectos ─────────────────────────────────────────────────
        row = EscribirSeccion(ws, row, "(-) GASTOS INDIRECTOS", dto.GastosIndirectos);
        row = EscribirTotalPrincipal(ws, row, "Total Gastos Indirectos", dto.TotalGastosIndirectos);
        row++;

        // ── Gastos Operativos ─────────────────────────────────────────────────
        row = EscribirSeccion(ws, row, "(-) GASTOS OPERATIVOS", dto.GastosOperativos);
        row = EscribirTotalPrincipal(ws, row, "Total Gastos Operativos", dto.TotalGastosOperativos);
        row++;

        // ── Intereses Préstamo ────────────────────────────────────────────────
        ws.Cell(row, 1).Value = "(-) Intereses préstamos";
        ws.Cell(row, 3).Value = dto.TotalInteresesPrestamo;
        FormatearMoneda(ws.Cell(row, 3));
        row++;
        row++;

        // ── Utilidad Operativa ────────────────────────────────────────────────
        row = EscribirSubtotal(ws, row, "UTILIDAD OPERATIVA", dto.UtilidadOperativa);
        row++;

        // ── Inversiones ───────────────────────────────────────────────────────
        row = EscribirSeccion(ws, row, "(-) INVERSIONES", dto.Inversiones);
        row = EscribirTotalPrincipal(ws, row, "Total Inversiones", dto.TotalInversiones);
        row++;

        // ── Utilidad Neta ─────────────────────────────────────────────────────
        row = EscribirSubtotal(ws, row, "UTILIDAD NETA", dto.UtilidadNeta, isFinal: true);

        // Formato columnas
        ws.Column(1).Width = 38;
        ws.Column(2).Width = 16;
        ws.Column(3).Width = 20;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Flujo de Caja ─────────────────────────────────────────────────────────

    public static byte[] GenerarFlujoCaja(FlujoCajaDto dto)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Flujo de Caja");

        var origen = string.IsNullOrWhiteSpace(dto.Origen) ? "Todos los orígenes" : dto.Origen;

        ws.Cell("A1").Value = "SmartFeedLot — Flujo de Caja";
        ws.Cell("A2").Value = $"Año: {dto.Anio}  |  Origen: {origen}";
        ws.Cell("A3").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Range("A1:E1").Merge().Style.Font.Bold = true;
        ws.Range("A1:E1").Style.Font.FontSize = 14;

        // Encabezados
        int row = 5;
        ws.Cell(row, 1).Value = "Mes";
        ws.Cell(row, 2).Value = "Ingresos";
        ws.Cell(row, 3).Value = "Egresos";
        ws.Cell(row, 4).Value = "Saldo Neto";
        ws.Cell(row, 5).Value = "Saldo Acumulado";
        var headerRange = ws.Range(row, 1, row, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a7a4a");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;

        foreach (var mes in dto.Meses)
        {
            ws.Cell(row, 1).Value = mes.NombreMes;
            ws.Cell(row, 2).Value = mes.Ingresos;
            ws.Cell(row, 3).Value = mes.Egresos;
            ws.Cell(row, 4).Value = mes.SaldoNeto;
            ws.Cell(row, 5).Value = mes.SaldoAcumulado;

            FormatearMoneda(ws.Cell(row, 2));
            FormatearMoneda(ws.Cell(row, 3));
            FormatearMoneda(ws.Cell(row, 4));
            FormatearMoneda(ws.Cell(row, 5));

            // Color negativo en rojo
            if (mes.SaldoNeto < 0)
                ws.Cell(row, 4).Style.Font.FontColor = XLColor.Red;
            if (mes.SaldoAcumulado < 0)
                ws.Cell(row, 5).Style.Font.FontColor = XLColor.Red;

            if (row % 2 == 0)
                ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#f0faf5");

            row++;
        }

        // Totales
        ws.Cell(row, 1).Value = "TOTAL";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = dto.TotalIngresos;
        ws.Cell(row, 3).Value = dto.TotalEgresos;
        ws.Cell(row, 4).Value = dto.SaldoNeto;
        FormatearMoneda(ws.Cell(row, 2));
        FormatearMoneda(ws.Cell(row, 3));
        FormatearMoneda(ws.Cell(row, 4));
        ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#e6f4ed");
        ws.Range(row, 1, row, 5).Style.Font.Bold = true;
        if (dto.SaldoNeto < 0)
            ws.Cell(row, 4).Style.Font.FontColor = XLColor.Red;

        ws.Column(1).Width = 12;
        ws.Column(2).Width = 20;
        ws.Column(3).Width = 20;
        ws.Column(4).Width = 20;
        ws.Column(5).Width = 22;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static int EscribirSeccion(
        IXLWorksheet ws, int row, string titulo,
        IReadOnlyList<LineaResultadoDto> lineas, bool isIngreso = false)
    {
        ws.Cell(row, 1).Value = titulo;
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#e8f5ee");
        row++;

        foreach (var linea in lineas)
        {
            ws.Cell(row, 2).Value = linea.Concepto;
            ws.Cell(row, 3).Value = linea.Monto;
            FormatearMoneda(ws.Cell(row, 3));
            row++;
        }

        if (!lineas.Any())
        {
            ws.Cell(row, 2).Value = "(Sin movimientos)";
            ws.Cell(row, 2).Style.Font.Italic = true;
            ws.Cell(row, 3).Value = 0m;
            FormatearMoneda(ws.Cell(row, 3));
            row++;
        }

        return row;
    }

    private static int EscribirTotalPrincipal(IXLWorksheet ws, int row, string label, decimal monto)
    {
        ws.Cell(row, 2).Value = label;
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = monto;
        FormatearMoneda(ws.Cell(row, 3));
        ws.Cell(row, 3).Style.Font.Bold = true;
        return row + 1;
    }

    private static int EscribirSubtotal(
        IXLWorksheet ws, int row, string label, decimal monto, bool isFinal = false)
    {
        var color = isFinal ? XLColor.FromHtml("#1a7a4a") : XLColor.FromHtml("#2d8f5e");
        ws.Cell(row, 1).Value = label;
        ws.Cell(row, 3).Value = monto;
        FormatearMoneda(ws.Cell(row, 3));
        var range = ws.Range(row, 1, row, 3);
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = color;
        range.Style.Font.FontColor = XLColor.White;
        if (monto < 0)
            ws.Cell(row, 3).Style.Font.FontColor = XLColor.FromHtml("#ffcccc");
        return row + 1;
    }

    private static void FormatearMoneda(IXLCell cell)
        => cell.Style.NumberFormat.Format = "#,##0.00";

    // ── Comparativo Presupuesto ───────────────────────────────────────────────

    public static byte[] GenerarComparativoPresupuesto(ComparativoPresupuestoDto dto)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Presupuesto vs Real");

        var periodo = dto.Mes.HasValue ? $"{NombreMes(dto.Mes.Value)} {dto.Anio}" : $"Año {dto.Anio}";
        ws.Cell("A1").Value = "SmartFeedLot — Presupuesto vs Real";
        ws.Cell("A2").Value = $"Período: {periodo}  |  Ejecución global: {dto.PorcentajeEjecucion}%";
        ws.Cell("A3").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Range("A1:F1").Merge().Style.Font.Bold = true;
        ws.Range("A1:F1").Style.Font.FontSize = 14;

        int row = 5;
        string[] headers = ["Categoría", "Tipo", "Presupuestado", "Real", "Desviación", "% Ejecución"];
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cell(row, c + 1).Value = headers[c];
        }
        var headerRange = ws.Range(row, 1, row, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a7a4a");
        headerRange.Style.Font.FontColor = XLColor.White;
        row++;

        foreach (var l in dto.Lineas)
        {
            ws.Cell(row, 1).Value = l.CategoriaNombre;
            ws.Cell(row, 2).Value = l.CategoriaTipo;
            ws.Cell(row, 3).Value = l.Presupuestado;
            ws.Cell(row, 4).Value = l.Real;
            ws.Cell(row, 5).Value = l.Desviacion;
            ws.Cell(row, 6).Value = $"{l.PorcentajeEjecucion}%";

            FormatearMoneda(ws.Cell(row, 3));
            FormatearMoneda(ws.Cell(row, 4));
            FormatearMoneda(ws.Cell(row, 5));

            var semaforoColor = l.Semaforo switch
            {
                "verde" => XLColor.FromHtml("#d4edda"),
                "amarillo" => XLColor.FromHtml("#fff3cd"),
                _ => XLColor.FromHtml("#f8d7da")
            };
            ws.Range(row, 1, row, 6).Style.Fill.BackgroundColor = semaforoColor;

            if (l.Desviacion > 0)
                ws.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
            else if (l.Desviacion < 0)
                ws.Cell(row, 5).Style.Font.FontColor = XLColor.FromHtml("#155724");

            row++;
        }

        // Totales
        ws.Cell(row, 1).Value = "TOTAL";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = dto.TotalPresupuestado;
        ws.Cell(row, 4).Value = dto.TotalReal;
        ws.Cell(row, 5).Value = dto.TotalDesviacion;
        ws.Cell(row, 6).Value = $"{dto.PorcentajeEjecucion}%";
        FormatearMoneda(ws.Cell(row, 3));
        FormatearMoneda(ws.Cell(row, 4));
        FormatearMoneda(ws.Cell(row, 5));
        ws.Range(row, 1, row, 6).Style.Font.Bold = true;
        ws.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#e6f4ed");

        ws.Column(1).Width = 32;
        ws.Column(2).Width = 14;
        ws.Column(3).Width = 20;
        ws.Column(4).Width = 20;
        ws.Column(5).Width = 20;
        ws.Column(6).Width = 14;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string NombreMes(int mes) => mes switch
    {
        1 => "Enero", 2 => "Febrero", 3 => "Marzo", 4 => "Abril",
        5 => "Mayo", 6 => "Junio", 7 => "Julio", 8 => "Agosto",
        9 => "Septiembre", 10 => "Octubre", 11 => "Noviembre", 12 => "Diciembre",
        _ => mes.ToString()
    };
}
