using Feedlot.Application.Features.Lotes.Commands.CrearLote;
using Feedlot.Application.Features.Lotes.Commands.MoverAnimalALote;
using Feedlot.Application.Features.Lotes.Queries.ObtenerLotePorId;
using Feedlot.Application.Features.Lotes.Queries.ObtenerLotes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class LotesController : ApiControllerBase
{
    private readonly ISender _sender;

    public LotesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Obtiene todos los lotes. Filtra por activos si se especifica.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerLotes(
        [FromQuery] bool soloActivos = false,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerLotesQuery(soloActivos), ct);
        return FromResult(result);
    }

    /// <summary>Obtiene el detalle completo de un lote incluyendo sus animales.</summary>
    [HttpGet("{id:guid}", Name = "ObtenerLotePorId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerLotePorIdQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>Crea un nuevo lote de engorde.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearLoteCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        return CreatedFromResult(result, "ObtenerLotePorId", new { id = result.Value });
    }

    /// <summary>Mueve un animal de su lote actual a otro lote destino.</summary>
    [HttpPost("{id:guid}/mover-animal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> MoverAnimal(
        Guid id,
        [FromBody] MoverAnimalRequest request,
        CancellationToken ct = default)
    {
        var command = new MoverAnimalALoteCommand(
            request.AnimalId, id, request.FechaMovimiento, request.Motivo);
        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }
}

public sealed record MoverAnimalRequest(
    Guid AnimalId,
    DateOnly FechaMovimiento,
    string Motivo);
