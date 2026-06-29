using Feedlot.Application.Features.Lotes.Commands.ActivarLote;
using Feedlot.Application.Features.Lotes.Commands.CerrarLote;
using Feedlot.Application.Features.Lotes.Commands.CrearLote;
using Feedlot.Application.Features.Lotes.Commands.MoverAnimalALote;
using Feedlot.Application.Features.Lotes.Queries.ObtenerLotePorId;
using Feedlot.Application.Features.Lotes.Queries.ObtenerLotes;
using Feedlot.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class LotesController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly ILoteRepository _loteRepository;

    public LotesController(ISender sender, ILoteRepository loteRepository)
    {
        _sender = sender;
        _loteRepository = loteRepository;
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

    /// <summary>
    /// Dado un conjunto de IDs de animales, retorna el lote activo de cada uno.
    /// Usado por el flujo de venta para detectar qué animales tienen lote antes de vender.
    /// POST porque los IDs pueden ser muchos para enviarlos en query string.
    /// Responde: { animalId → { loteId, loteCodigo } }
    /// </summary>
    [HttpPost("consultar-lotes-animales")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultarLotesAnimales(
        [FromBody] ConsultarLotesAnimalesRequest request,
        CancellationToken ct = default)
    {
        // Cargar todos los lotes activos con sus AnimalesLote de una sola query.
        var lotesActivos = await _loteRepository.ObtenerActivosAsync(ct);

        // Construir el mapa animalId → { loteId, loteCodigo } en memoria.
        var resultado = new Dictionary<string, object>();

        foreach (var animalId in request.AnimalIds.Distinct())
        {
            var lote = lotesActivos.FirstOrDefault(l =>
                l.AnimalesLote.Any(al => al.AnimalId == animalId && al.EsActivo));

            if (lote is not null)
            {
                resultado[animalId.ToString()] = new
                {
                    loteId = lote.Id,
                    loteCodigo = lote.Codigo,
                    loteNombre = lote.Nombre,
                };
            }
        }

        return Ok(resultado);
    }

    /// <summary>Crea un nuevo lote de engorde en estado EnPreparacion.</summary>
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

    /// <summary>Activa un lote en estado EnPreparacion.</summary>
    [HttpPut("{id:guid}/activar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct = default)
    {
        var result = await _sender.Send(new ActivarLoteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>Cierra un lote activo sin animales.</summary>
    [HttpPut("{id:guid}/cerrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cerrar(Guid id, CancellationToken ct = default)
    {
        var result = await _sender.Send(new CerrarLoteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>Mueve un animal de su lote actual a este lote destino.</summary>
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

public sealed record ConsultarLotesAnimalesRequest(List<Guid> AnimalIds);
