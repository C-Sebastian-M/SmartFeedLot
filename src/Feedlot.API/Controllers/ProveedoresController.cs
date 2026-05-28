using Feedlot.Application.Features.Proveedores.Commands.CrearProveedor;
using Feedlot.Application.Features.Proveedores.Commands.ModificarProveedor;
using Feedlot.Application.Features.Proveedores.Commands.EliminarProveedor;
using Feedlot.Application.Features.Proveedores.Queries.ObtenerProveedores;
using Feedlot.Application.Features.Proveedores.Queries.ObtenerProveedorPorId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class ProveedoresController : ApiControllerBase
{
    private readonly ISender _sender;

    public ProveedoresController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerProveedoresQuery(), ct);
        return FromResult(result);
    }

    [HttpGet("{id:guid}", Name = "ObtenerProveedorPorId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerProveedorPorIdQuery(id), ct);
        return FromResult(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearProveedorCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedFromResult(result, "ObtenerProveedorPorId", new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Modificar(Guid id, [FromBody] ModificarProveedorCommand command, CancellationToken ct)
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
        var result = await _sender.Send(new EliminarProveedorCommand(id), ct);
        return FromResult(result);
    }
}
