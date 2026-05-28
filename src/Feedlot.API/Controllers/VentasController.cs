using Feedlot.Application.Features.Ventas.Commands.CrearVenta;
using Feedlot.Application.Features.Ventas.Queries.ObtenerVentas;
using Feedlot.Application.Features.Ventas.Queries.ObtenerVentaPorId;
using Feedlot.Application.Features.Ventas.Queries.ObtenerCompradores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class VentasController : ApiControllerBase
{
    private readonly ISender _sender;

    public VentasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerVentasQuery(), ct);
        return FromResult(result);
    }

    [HttpGet("{id:guid}", Name = "ObtenerVentaPorId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerVentaPorIdQuery(id), ct);
        return FromResult(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearVentaCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedFromResult(result, "ObtenerVentaPorId", new { id = result.Value });
    }

    // ─── Compradores ──────────────────────────────────────────────────────────

    [HttpGet("compradores")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCompradores(CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerCompradoresQuery(), ct);
        return FromResult(result);
    }
}
