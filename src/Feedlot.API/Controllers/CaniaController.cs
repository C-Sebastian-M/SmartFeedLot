using Feedlot.Application.Features.Operacion.Commands.CrearCultivoCania;
using Feedlot.Application.Features.Operacion.Commands.RegistrarCorteCania;
using Feedlot.Application.Features.Operacion.Commands.CrearLoteSilo;
using Feedlot.Application.Features.Operacion.Queries.ObtenerCultivosCania;
using Feedlot.Application.Features.Operacion.Queries.ObtenerLotesSilo;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class CaniaController : ApiControllerBase
{
    private readonly ISender _sender;
    public CaniaController(ISender sender) => _sender = sender;

    [HttpGet("cultivos")]
    public async Task<IActionResult> ObtenerCultivos(CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerCultivosCaniaQuery(), ct);
        return FromResult(result);
    }

    [HttpPost("cultivos")]
    public async Task<IActionResult> CrearCultivo([FromBody] CrearCultivoCaniaCommand command, CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerCultivos), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    [HttpPost("cultivos/{cultivoId:guid}/cortes")]
    public async Task<IActionResult> RegistrarCorte(Guid cultivoId, [FromBody] RegistrarCorteCaniaCommand command, CancellationToken ct = default)
    {
        if (cultivoId != command.CultivoCaniaId) return BadRequest("ID del cultivo no coincide.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : FromResult(result);
    }

    [HttpGet("lotes-silo")]
    public async Task<IActionResult> ObtenerLotesSilo([FromQuery] bool? soloDisponibles, CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerLotesSiloQuery(soloDisponibles), ct);
        return FromResult(result);
    }

    [HttpPost("lotes-silo")]
    public async Task<IActionResult> CrearLoteSilo([FromBody] CrearLoteSiloCommand command, CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerLotesSilo), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }
}
