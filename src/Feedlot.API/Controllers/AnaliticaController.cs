using Feedlot.Application.Features.Analitica.Queries.ObtenerAnimalesIneficientes;
using Feedlot.Application.Features.Analitica.Queries.ObtenerIndicadoresAnimal;
using Feedlot.Application.Features.Analitica.Queries.ObtenerResumenLote;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

/// <summary>
/// Controller de Analítica — expone los endpoints del motor productivo.
/// Todas las queries son read-only: no modifican estado.
/// </summary>
[Authorize]
public sealed class AnaliticaController : ApiControllerBase
{
    private readonly ISender _sender;

    public AnaliticaController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Calcula GMD, ICA, costo por kg y rentabilidad proyectada de un animal
    /// en el período especificado.
    /// </summary>
    [HttpGet("animales/{animalId:guid}/indicadores")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerIndicadoresAnimal(
        Guid animalId,
        [FromQuery] Guid loteId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        [FromQuery] decimal precioVentaEstimadoPorKg = 5500,
        CancellationToken ct = default)
    {
        var query = new ObtenerIndicadoresAnimalQuery(
            animalId, loteId, desde, hasta, precioVentaEstimadoPorKg);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Resumen ejecutivo de un lote: agrega indicadores de todos sus animales,
    /// consumo total, GMD promedio, ICA promedio y rentabilidad total.
    /// Alimenta el dashboard principal.
    /// </summary>
    [HttpGet("lotes/{loteId:guid}/resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerResumenLote(
        Guid loteId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        [FromQuery] decimal precioVentaEstimadoPorKg = 5500,
        CancellationToken ct = default)
    {
        var query = new ObtenerResumenLoteQuery(
            loteId, desde, hasta, precioVentaEstimadoPorKg);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna la lista de animales que están por debajo de los umbrales
    /// productivos mínimos. Ordenados por GMD ascendente (más críticos primero).
    /// </summary>
    [HttpGet("animales-ineficientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerAnimalesIneficientes(
        [FromQuery] Guid? loteId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        [FromQuery] decimal precioVentaEstimadoPorKg = 5500,
        [FromQuery] decimal gmdMinima = 0.8m,
        [FromQuery] decimal icaMaxima = 8.0m,
        CancellationToken ct = default)
    {
        var query = new ObtenerAnimalesIneficientesQuery(
            loteId, desde, hasta, precioVentaEstimadoPorKg, gmdMinima, icaMaxima);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }
}
