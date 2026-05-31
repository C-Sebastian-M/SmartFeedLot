using Feedlot.Application.Features.Finanzas.Commands.AnularPagoCuota;
using Feedlot.Application.Features.Finanzas.Commands.CrearPrestamo;
using Feedlot.Application.Features.Finanzas.Commands.RegistrarPagoCuota;
using Feedlot.Application.Features.Finanzas.Queries.ObtenerPrestamos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class PrestamosController : ApiControllerBase
{
    private readonly ISender _sender;
    public PrestamosController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> ObtenerPrestamos(CancellationToken ct = default)
    {
        var result = await _sender.Send(new ObtenerPrestamosQuery(), ct);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPrestamoCommand command, CancellationToken ct = default)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(ObtenerPrestamos), new { id = result.Value }, new { id = result.Value });
        return FromResult(result);
    }

    /// <summary>Registra el pago de una cuota.</summary>
    [HttpPatch("{prestamoId:guid}/cuotas/{cuotaId:guid}/pagar")]
    public async Task<IActionResult> RegistrarPago(
        Guid prestamoId, Guid cuotaId,
        [FromBody] RegistrarPagoRequest request,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new RegistrarPagoCuotaCommand(prestamoId, cuotaId, request.FechaPago), ct);
        return FromResult(result);
    }

    /// <summary>Anula el pago de una cuota.</summary>
    [HttpPatch("{prestamoId:guid}/cuotas/{cuotaId:guid}/anular")]
    public async Task<IActionResult> AnularPago(
        Guid prestamoId, Guid cuotaId,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new AnularPagoCuotaCommand(prestamoId, cuotaId), ct);
        return FromResult(result);
    }
}

public sealed record RegistrarPagoRequest(DateOnly FechaPago);
