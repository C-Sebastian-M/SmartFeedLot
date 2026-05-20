using Feedlot.Application.Features.Nutricion.Commands.CrearRacion;
using Feedlot.Application.Features.Nutricion.Commands.RegistrarConsumo;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class NutricionController : ApiControllerBase
{
    private readonly ISender _sender;

    public NutricionController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Crea una nueva ración alimenticia.</summary>
    [HttpPost("raciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearRacion(
        [FromBody] CrearRacionCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(CrearRacion), new { id = result.Value }, result.Value);
        return FromResult(result);
    }

    /// <summary>Registra el consumo alimenticio diario de un lote.</summary>
    [HttpPost("consumos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarConsumo(
        [FromBody] RegistrarConsumoCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(RegistrarConsumo), new { id = result.Value }, result.Value);
        return FromResult(result);
    }
}
