using Feedlot.Application.Features.Compradores.Commands.CrearComprador;
using Feedlot.Application.Features.Compradores.Commands.ModificarComprador;
using Feedlot.Application.Features.Compradores.Commands.EliminarComprador;
using Feedlot.Application.Features.Ventas.Queries.ObtenerCompradores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class CompradoresController : ApiControllerBase
{
    private readonly ISender _sender;

    public CompradoresController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerCompradoresQuery(), ct);
        return FromResult(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearCompradorCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedFromResult(result, null, null!);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Modificar(Guid id, [FromBody] ModificarCompradorCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la ruta no coincide con el ID del cuerpo." });
        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new EliminarCompradorCommand(id), ct);
        return FromResult(result);
    }
}
