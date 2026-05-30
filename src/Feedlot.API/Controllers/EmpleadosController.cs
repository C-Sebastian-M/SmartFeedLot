using Feedlot.Application.Features.Operacion.Commands.CrearEmpleado;
using Feedlot.Application.Features.Operacion.Commands.RegistrarActividadManoObra;
using Feedlot.Application.Features.Operacion.Queries.ObtenerEmpleados;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class EmpleadosController : ApiControllerBase
{
    private readonly ISender _sender;
    public EmpleadosController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> ObtenerEmpleados(CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerEmpleadosQuery(), ct);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CrearEmpleado([FromBody] CrearEmpleadoCommand command, CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerEmpleados), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    [HttpPost("{empleadoId:guid}/actividades")]
    public async Task<IActionResult> RegistrarActividad(Guid empleadoId, [FromBody] RegistrarActividadManoObraCommand command, CancellationToken ct = default)
    {
        if (empleadoId != command.EmpleadoId) return BadRequest("ID del empleado no coincide.");
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : FromResult(result);
    }
}
