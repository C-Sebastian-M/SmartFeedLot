using Feedlot.API.Services;
using Feedlot.Application.Features.Costos.Queries.ObtenerCostosTotalesLote;
using Feedlot.Application.Features.Finanzas.Commands.CrearCategoriaGasto;
using Feedlot.Application.Features.Finanzas.Commands.CrearSocio;
using Feedlot.Application.Features.Finanzas.Commands.RegistrarMovimiento;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerCategoriasGasto;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerEstadoResultados;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerFlujoCaja;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerMovimientosFinancieros;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerSocios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class FinanzasController : ApiControllerBase
{
    private readonly ISender _sender;

    public FinanzasController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Desglose completo de costos del lote en un período (MP + MO + CIF prorrateados).
    /// </summary>
    [HttpGet("lotes/{loteId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerCosteoLote(
        Guid loteId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        CancellationToken ct = default)
    {
        var query = new ObtenerCostosTotalesLoteQuery(loteId, desde, hasta);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Obtiene los movimientos financieros aplicando filtros.
    /// </summary>
    [HttpGet("movimientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerMovimientos(
        [FromQuery] int? anio,
        [FromQuery] int? mes,
        [FromQuery] string? origen,
        [FromQuery] Guid? categoriaGastoId,
        [FromQuery] Guid? socioId,
        CancellationToken ct = default)
    {
        var query = new ObtenerMovimientosFinancierosQuery(anio, mes, origen, categoriaGastoId, socioId);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Registra un nuevo movimiento financiero.
    /// </summary>
    [HttpPost("movimientos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarMovimiento(
        [FromBody] RegistrarMovimientoFinancieroCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerMovimientos), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    /// <summary>
    /// Obtiene la lista de categorías de gasto.
    /// </summary>
    [HttpGet("categorias")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCategorias(CancellationToken ct = default)
    {
        var query = new ObtenerCategoriasGastoQuery();
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea una nueva categoría de gasto.
    /// </summary>
    [HttpPost("categorias")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearCategoria(
        [FromBody] CrearCategoriaGastoCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerCategorias), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    /// <summary>
    /// Obtiene la lista de socios.
    /// </summary>
    [HttpGet("socios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerSocios(CancellationToken ct = default)
    {
        var query = new ObtenerSociosQuery();
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea un nuevo socio.
    /// </summary>
    [HttpPost("socios")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearSocio(
        [FromBody] CrearSocioCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerSocios), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    // ── Reportes financieros ─────────────────────────────────────────────────

    /// <summary>
    /// Estado de Resultados (P&amp;L) para un año, opcionalmente filtrado por mes y origen.
    /// </summary>
    [HttpGet("estado-resultados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObtenerEstadoResultados(
        [FromQuery] int anio,
        [FromQuery] int? mes,
        [FromQuery] string? origen,
        CancellationToken ct = default)
    {
        var query = new ObtenerEstadoResultadosQuery(anio, mes, origen);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Estado de Resultados exportado a Excel (.xlsx).
    /// </summary>
    [HttpGet("estado-resultados/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportarEstadoResultados(
        [FromQuery] int anio,
        [FromQuery] int? mes,
        [FromQuery] string? origen,
        CancellationToken ct = default)
    {
        var query = new ObtenerEstadoResultadosQuery(anio, mes, origen);
        var result = await _sender.Send(query, ct);
        if (result.IsFailure) return FromResult(result);

        var bytes = ExcelExportService.GenerarEstadoResultados(result.Value!);
        var periodoStr = mes.HasValue ? $"{mes:D2}-{anio}" : $"{anio}";
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"EstadoResultados_{periodoStr}.xlsx");
    }

    /// <summary>
    /// Flujo de caja mensual para un año.
    /// </summary>
    [HttpGet("flujo-caja")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObtenerFlujoCaja(
        [FromQuery] int anio,
        [FromQuery] string? origen,
        CancellationToken ct = default)
    {
        var query = new ObtenerFlujoCajaQuery(anio, origen);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Flujo de caja exportado a Excel (.xlsx).
    /// </summary>
    [HttpGet("flujo-caja/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportarFlujoCaja(
        [FromQuery] int anio,
        [FromQuery] string? origen,
        CancellationToken ct = default)
    {
        var query = new ObtenerFlujoCajaQuery(anio, origen);
        var result = await _sender.Send(query, ct);
        if (result.IsFailure) return FromResult(result);

        var bytes = ExcelExportService.GenerarFlujoCaja(result.Value!);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"FlujoCaja_{anio}.xlsx");
    }
}
