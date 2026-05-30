using Feedlot.Application.Features.Finanzas.Commands.CrearPrestamo;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerPrestamos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class PrestamosController : ApiControllerBase
{
    private readonly ISender _sender;

    public PrestamosController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Obtiene la lista de todos los préstamos registrados con sus tablas de amortización.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPrestamos(CancellationToken ct = default)
    {
        var query = new ObtenerPrestamosQuery();
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea un nuevo préstamo y genera su tabla de amortización.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearPrestamoCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerPrestamos), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }
}
