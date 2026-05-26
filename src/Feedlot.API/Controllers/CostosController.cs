using Feedlot.Application.Features.Costos.Commands.RegistrarCostoOperativo;
using Feedlot.Application.Features.Costos.Queries.ObtenerCostosTotalesLote;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class CostosController : ApiControllerBase
{
    private readonly ISender _sender;

    public CostosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("costo-operativo")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarCostoOperativo(
        [FromBody] RegistrarCostoOperativoCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(RegistrarCostoOperativo), new { id = result.Value }, result.Value);
        return FromResult(result);
    }

    [HttpGet("lotes/{loteId:guid}/costos-totales")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerCostosTotalesLote(
        Guid loteId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        CancellationToken ct = default)
    {
        var query = new ObtenerCostosTotalesLoteQuery(loteId, desde, hasta);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }
}
