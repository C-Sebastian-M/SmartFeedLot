using Feedlot.Application.Features.Porcino.Commands.CrearLoteCerdos;
using Feedlot.Application.Features.Porcino.Commands.RegistrarVentaLoteCerdos;
using Feedlot.Application.Features.Porcino.Queries.ObtenerLotesCerdos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
[Route("api/lotes-cerdos")]
public sealed class LotesCerdosController : ApiControllerBase
{
    private readonly ISender _sender;
    public LotesCerdosController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> ObtenerLotesCerdos(CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerLotesCerdosQuery(), ct);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CrearLoteCerdos([FromBody] CrearLoteCerdosCommand command, CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerLotesCerdos), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    [HttpPost("{loteId:guid}/vender")]
    public async Task<IActionResult> RegistrarVenta(Guid loteId, [FromBody] RegistrarVentaLoteCerdosCommand command, CancellationToken ct = default)
    {
        if (loteId != command.LoteId) return BadRequest("ID del lote no coincide.");
        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }
}
