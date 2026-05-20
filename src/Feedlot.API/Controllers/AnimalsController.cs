using Feedlot.Application.Features.Animals.Commands.RegistrarAnimal;
using Feedlot.Application.Features.Animals.Commands.RegistrarEventoSanitario;
using Feedlot.Application.Features.Animals.Commands.RegistrarPesaje;
using Feedlot.Application.Features.Animals.Queries.ObtenerAnimalPorId;
using Feedlot.Application.Features.Animals.Queries.ObtenerAnimales;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

/// <summary>
/// Controller de Animals. Thin controller — solo delega a MediatR.
/// No contiene lógica de negocio. No instancia objetos de dominio directamente.
/// </summary>
[Authorize]
public sealed class AnimalsController : ApiControllerBase
{
    private readonly ISender _sender;

    public AnimalsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Obtiene lista paginada de animales con filtros opcionales.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerAnimales(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? estadoProductivo = null,
        [FromQuery] string? estadoSanitario = null,
        [FromQuery] string? raza = null,
        [FromQuery] string? busqueda = null,
        CancellationToken ct = default)
    {
        var query = new ObtenerAnimalesQuery(
            page, pageSize, estadoProductivo, estadoSanitario, raza, busqueda);
        var result = await _sender.Send(query, ct);
        return FromResult(result);
    }

    /// <summary>Obtiene el detalle completo de un animal por ID.</summary>
    [HttpGet("{id:guid}", Name = "ObtenerAnimalPorId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerAnimalPorIdQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>Registra un nuevo animal en el sistema.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarAnimalCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        return CreatedFromResult(result, "ObtenerAnimalPorId", new { id = result.Value });
    }

    /// <summary>Registra un nuevo pesaje sobre un animal.</summary>
    [HttpPost("{id:guid}/pesajes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarPesaje(
        Guid id,
        [FromBody] RegistrarPesajeRequest request,
        CancellationToken ct = default)
    {
        var command = new RegistrarPesajeCommand(
            id, request.FechaPesaje, request.PesoKg, request.Observaciones);
        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>Registra un evento sanitario sobre un animal.</summary>
    [HttpPost("{id:guid}/eventos-sanitarios")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarEventoSanitario(
        Guid id,
        [FromBody] RegistrarEventoSanitarioRequest request,
        CancellationToken ct = default)
    {
        var command = new RegistrarEventoSanitarioCommand(
            id,
            request.FechaEvento,
            request.Diagnostico,
            request.Descripcion,
            request.Severidad,
            request.Tratamiento);
        var result = await _sender.Send(command, ct);
        return FromResult(result);
    }
}

// Request models separados del Command para que el ID venga de la ruta, no del body.
public sealed record RegistrarPesajeRequest(
    DateOnly FechaPesaje,
    decimal PesoKg,
    string? Observaciones);

public sealed record RegistrarEventoSanitarioRequest(
    DateOnly FechaEvento,
    string Diagnostico,
    string Descripcion,
    string Severidad,
    string? Tratamiento);
