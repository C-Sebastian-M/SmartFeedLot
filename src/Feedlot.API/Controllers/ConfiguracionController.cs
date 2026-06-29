using Feedlot.Application.Features.Configuracion.Commands.CambiarEstadoModulo;
using Feedlot.Application.Features.Configuracion.Queries.ObtenerModulos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
[Route("api/configuracion")]
public sealed class ConfiguracionController : ApiControllerBase
{
    private readonly ISender _sender;
    public ConfiguracionController(ISender sender) => _sender = sender;

    /// <summary>
    /// Lista los módulos del sistema con su estado. Cualquier usuario autenticado
    /// puede leerlo (el menú lo necesita para saber qué mostrar).
    /// </summary>
    [HttpGet("modulos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerModulos(CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerModulosQuery(), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Activa o desactiva un módulo. Solo el Admin puede hacerlo.
    /// </summary>
    [HttpPut("modulos/{clave}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstadoModulo(
        string clave,
        [FromBody] CambiarEstadoModuloRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new CambiarEstadoModuloCommand(clave, request.Activo), ct);
        return FromResult(result);
    }
}

public sealed record CambiarEstadoModuloRequest(bool Activo);
