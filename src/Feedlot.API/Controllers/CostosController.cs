using Feedlot.Application.Features.Costos.Commands.RegistrarCostoOperativo;
using Feedlot.Application.Features.Costos.Queries.ObtenerCostosTotalesLote;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

/// <summary>
/// Controller de Costos Operativos (Mano de Obra y CIF).
/// Endpoints:
///   GET  api/costos/lotes/{loteId}?desde=&hasta=   → desglose completo
///   GET  api/costos/lotes/{loteId}/detalle          → lista de registros
///   POST api/costos                                 → registrar MO o CIF
///   DELETE api/costos/{id}                          → eliminar registro
/// </summary>
[Authorize]
public sealed class CostosController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly ICostoOperativoRepository _costoRepo;

    public CostosController(ISender sender, ICostoOperativoRepository costoRepo)
    {
        _sender = sender;
        _costoRepo = costoRepo;
    }

    /// <summary>
    /// Desglose completo de costos del lote en un período:
    /// alimento (MP) + mano de obra + CIF con totales y prorrateo por animal.
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
    /// Lista detallada de registros de MO y CIF de un lote.
    /// Filtrable por categoría: ManoDeObra | CIF
    /// </summary>
    [HttpGet("lotes/{loteId:guid}/detalle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerDetalle(
        Guid loteId,
        [FromQuery] string? categoria = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        CategoriaCosto? cat = categoria is not null
            && Enum.TryParse<CategoriaCosto>(categoria, ignoreCase: true, out var parsed)
            ? parsed : null;

        var costos = await _costoRepo.ObtenerPorLoteAsync(loteId, desde, hasta, cat, ct);

        var dto = costos.Select(c => new
        {
            id = c.Id,
            loteId = c.LoteId,
            categoria = c.Categoria.ToString(),
            concepto = c.Concepto,
            fecha = c.Fecha,
            monto = c.Monto.Monto,
            moneda = c.Monto.Moneda,
            observaciones = c.Observaciones,
        });

        return Ok(dto);
    }

    /// <summary>
    /// Registra un nuevo costo operativo (Mano de Obra o CIF) en un lote.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarCostoOperativoCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerDetalle),
                new { loteId = command.LoteId }, new { id = result.Value });
        return FromResult(result);
    }
}
