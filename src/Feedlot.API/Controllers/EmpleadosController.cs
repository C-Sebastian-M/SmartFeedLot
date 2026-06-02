using Feedlot.Application.Features.Operacion.Commands.CrearEmpleado;
using Feedlot.Application.Features.Operacion.Commands.EliminarEmpleado;
using Feedlot.Application.Features.Operacion.Commands.ModificarActividad;
using Feedlot.Application.Features.Operacion.Commands.ModificarEmpleado;
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

    [HttpPut("{empleadoId:guid}")]
    public async Task<IActionResult> ModificarEmpleado(Guid empleadoId, [FromBody] ModificarEmpleadoCommand command, CancellationToken ct = default)
    {
        if (empleadoId != command.EmpleadoId) return BadRequest("ID del empleado no coincide.");
        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }

    [HttpDelete("{empleadoId:guid}")]
    public async Task<IActionResult> EliminarEmpleado(Guid empleadoId, CancellationToken ct = default)
    {
        var result = await _sender.Send(new EliminarEmpleadoCommand(empleadoId), ct);
        return FromResult(result);
    }

    [HttpPatch("{empleadoId:guid}/actividades/{actividadId:guid}")]
    public async Task<IActionResult> ModificarActividad(Guid empleadoId, Guid actividadId, [FromBody] ModificarActividadCommand command, CancellationToken ct = default)
    {
        if (actividadId != command.ActividadId) return BadRequest("ID de la actividad no coincide.");
        var result = await _sender.Send(command, ct);
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
