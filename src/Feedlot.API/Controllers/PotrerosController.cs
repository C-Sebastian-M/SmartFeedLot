using Feedlot.Application.Features.Operacion.Commands.CrearPotrero;
using Feedlot.Application.Features.Operacion.Commands.EliminarPotrero;
using Feedlot.Application.Features.Operacion.Commands.IngresarAnimalPotrero;
using Feedlot.Application.Features.Operacion.Commands.RetirarAnimalPotrero;
using Feedlot.Application.Features.Operacion.Queries.ObtenerPotreros;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class PotrerosController : ApiControllerBase
{
    private readonly ISender _sender;
    public PotrerosController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> ObtenerPotreros(CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerPotrerosQuery(), ct);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CrearPotrero([FromBody] CrearPotreroCommand command, CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerPotreros), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    [HttpPost("{potreroId:guid}/ingresar")]
    public async Task<IActionResult> IngresarAnimal(Guid potreroId, [FromBody] IngresarAnimalPotreroCommand command, CancellationToken ct = default)
    {
        if (potreroId != command.PotreroId) return BadRequest("ID del potrero no coincide.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : FromResult(result);
    }

    [HttpPost("{potreroId:guid}/retirar")]
    public async Task<IActionResult> RetirarAnimal(Guid potreroId, [FromBody] RetirarAnimalPotreroCommand command, CancellationToken ct = default)
    {
        if (potreroId != command.PotreroId) return BadRequest("ID del potrero no coincide.");
        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }

    [HttpDelete("{potreroId:guid}")]
    public async Task<IActionResult> EliminarPotrero(Guid potreroId, CancellationToken ct = default)
    {
        var result = await _sender.Send(new EliminarPotreroCommand(potreroId), ct);
        return FromResult(result);
    }
}
