using Feedlot.Application.Features.Porcino.Commands.CrearMarrana;
using Feedlot.Application.Features.Porcino.Commands.RegistrarCamada;
using Feedlot.Application.Features.Porcino.Queries.ObtenerMarranas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class MarranasController : ApiControllerBase
{
    private readonly ISender _sender;
    public MarranasController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> ObtenerMarranas(CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerMarranasQuery(), ct);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CrearMarrana([FromBody] CrearMarranaCommand command, CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerMarranas), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    [HttpPost("{marranaId:guid}/camadas")]
    public async Task<IActionResult> RegistrarCamada(Guid marranaId, [FromBody] RegistrarCamadaCommand command, CancellationToken ct = default)
    {
        if (marranaId != command.MarranaId) return BadRequest("ID de la marrana no coincide.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : FromResult(result);
    }
}
