using Feedlot.Application.Features.Mercado.Commands.CrearPrecioMercado;
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
        return CreatedFromResult(result, nameof(ObtenerTodos), new { id = result.Value });
    }
}
