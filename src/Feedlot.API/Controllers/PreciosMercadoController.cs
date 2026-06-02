using Feedlot.Application.Features.Mercado.Commands.ActualizarPrecioMercado;
using Feedlot.Application.Features.Mercado.Commands.CrearPrecioMercado;
using Feedlot.Application.Features.Mercado.Commands.EliminarPrecioMercado;
using Feedlot.Application.Features.Mercado.Queries.ObtenerPreciosMercado;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
[Route("api/precios-mercado")]
public sealed class PreciosMercadoController : ApiControllerBase
{
    private readonly ISender _sender;

    public PreciosMercadoController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerPreciosMercadoQuery(), ct);
        return FromResult(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearPrecioMercadoCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        if (!result.IsSuccess) return FromResult(result);
        return Created($"/api/precios-mercado/{result.Value}", new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarPrecioMercadoCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("El ID no coincide.");
        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new EliminarPrecioMercadoCommand(id), ct);
        return FromResult(result);
    }
}
