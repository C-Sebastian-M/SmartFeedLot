using Feedlot.Application.Features.Nutricion.Commands.CrearRacion;
using Feedlot.Application.Features.Nutricion.Commands.RegistrarConsumo;
using Feedlot.Domain.Interfaces;
using AutoMapper;
using Feedlot.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

/// <summary>
/// Controller de Nutrición.
/// Ruta base: api/nutricion
/// Endpoints:
///   GET  api/nutricion/raciones        → lista raciones activas
///   POST api/nutricion/raciones        → crea ración
///   POST api/nutricion/consumos        → registra consumo diario
/// </summary>
[Authorize]
public sealed class NutricionController : ApiControllerBase
{
    private readonly ISender _sender;
    private readonly IRacionRepository _racionRepository;
    private readonly IMapper _mapper;

    public NutricionController(
        ISender sender,
        IRacionRepository racionRepository,
        IMapper mapper)
    {
        _sender = sender;
        _racionRepository = racionRepository;
        _mapper = mapper;
    }

    /// <summary>Lista todas las raciones activas.</summary>
    [HttpGet("raciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerRaciones(CancellationToken ct = default)
    {
        var raciones = await _racionRepository.ObtenerActivasAsync(ct);
        var dtos = _mapper.Map<IReadOnlyList<RacionDto>>(raciones);
        return Ok(dtos);
    }

    /// <summary>Crea una nueva ración alimenticia.</summary>
    [HttpPost("raciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearRacion(
        [FromBody] CrearRacionCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(CrearRacion), new { id = result.Value }, result.Value);
        return FromResult(result);
    }

    /// <summary>Registra el consumo alimenticio diario de un lote.</summary>
    [HttpPost("consumos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarConsumo(
        [FromBody] RegistrarConsumoCommand command,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(RegistrarConsumo), new { id = result.Value }, result.Value);
        return FromResult(result);
    }
}
