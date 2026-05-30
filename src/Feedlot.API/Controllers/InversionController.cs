using Feedlot.Application.Features.Inversion.Commands.ActualizarItemInversion;
using Feedlot.Application.Features.Inversion.Commands.AgregarItemInversion;
using Feedlot.Application.Features.Inversion.Commands.CrearAporteSocio;
using Feedlot.Application.Features.Inversion.Commands.CrearEtapaInversion;
using Feedlot.Application.Features.Inversion.Queries.ObtenerAportesSocios;
using Feedlot.Application.Features.Inversion.Queries.ObtenerEtapasInversion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class InversionController : ApiControllerBase
{
    private readonly ISender _sender;

    public InversionController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("etapas")]
    public async Task<IActionResult> ObtenerEtapas(CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerEtapasInversionQuery(), ct);
        return FromResult(result);
    }

    [HttpPost("etapas")]
    public async Task<IActionResult> CrearEtapa(
        [FromBody] CrearEtapaInversionCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerEtapas), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AgregarItem(
        [FromBody] AgregarItemInversionCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerEtapas), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    [HttpPatch("items/{itemId:guid}")]
    public async Task<IActionResult> ActualizarItem(
        Guid itemId,
        [FromBody] ActualizarItemInversionCommand command,
        CancellationToken ct = default)
    {
        if (itemId != command.ItemId)
            return BadRequest("El ID del ítem en la URL no coincide con el cuerpo de la solicitud.");

        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }

    [HttpPost("aportes")]
    public async Task<IActionResult> CrearAporte(
        [FromBody] CrearAporteSocioCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerAportes), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    [HttpGet("aportes")]
    public async Task<IActionResult> ObtenerAportes(
        [FromQuery] Guid? socioId,
        [FromQuery] Guid? itemInversionId,
        CancellationToken ct = default)
    {
        var query = new ObtenerAportesSociosQuery(socioId, itemInversionId);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }
}
