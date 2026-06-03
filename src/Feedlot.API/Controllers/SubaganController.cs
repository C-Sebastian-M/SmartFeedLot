using Feedlot.Application.Features.Mercado.Commands.EliminarSubaganEvento;
using Feedlot.Application.Features.Mercado.Commands.ImportarSubasta;
using Feedlot.Application.Features.Mercado.Queries.ObtenerSubaganCalendario;
using Feedlot.Application.Features.Mercado.Queries.ObtenerSubaganEventos;
using Feedlot.Application.Features.Mercado.Queries.ObtenerSubaganLotes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
[Route("api/subagan")]
public sealed class SubaganController : ApiControllerBase
{
    private readonly ISender _sender;
    public SubaganController(ISender sender) => _sender = sender;

    /// <summary>Lista todos los eventos de SUBAGAN importados.</summary>
    [HttpGet("eventos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEventos(CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerSubaganEventosQuery(), ct);
        return FromResult(result);
    }

    /// <summary>Lista los lotes de un evento específico.</summary>
    [HttpGet("eventos/{eventoId:guid}/lotes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerLotes(Guid eventoId, CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerSubaganLotesQuery(eventoId), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Lista los eventos del calendario de SUBAGAN para elegir cuál importar.
    /// Filtros opcionales: ?sede=PLANETA RICA y ?soloPasadas=true
    /// </summary>
    [HttpGet("calendario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ObtenerCalendario(
        [FromQuery] string? sede, [FromQuery] bool soloPasadas, CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerSubaganCalendarioQuery(sede, soloPasadas), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Importa todos los lotes de una subasta desde SUBAGAN.
    /// El eventId es el ID visible en la URL showLots?eventId=X de SUBAGAN.
    /// Si el evento ya fue importado, devuelve los datos existentes sin re-importar.
    /// </summary>
    [HttpPost("importar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Importar([FromBody] ImportarSubastaCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Elimina un evento de SUBAGAN importado (y sus lotes en cascada).
    /// El eventoId es el identificador interno (Guid), no el ID de SUBAGAN.
    /// </summary>
    [HttpDelete("eventos/{eventoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarEvento(Guid eventoId, CancellationToken ct)
    {
        var result = await _sender.Send(new EliminarSubaganEventoCommand(eventoId), ct);
        return FromResult(result);
    }
}
