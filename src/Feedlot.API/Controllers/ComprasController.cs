using Feedlot.Application.Features.Compras.Commands.CrearCompra;
using Feedlot.Application.Features.Compras.Queries.ObtenerCompras;
using Feedlot.Application.Features.Compras.Queries.ObtenerComprasPorProveedor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

[Authorize]
public sealed class ComprasController : ApiControllerBase
{
    private readonly ISender _sender;

    public ComprasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerComprasQuery(), ct);
        return FromResult(result);
    }

    [HttpGet("por-proveedor/{proveedorId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorProveedor(Guid proveedorId, CancellationToken ct)
    {
        var result = await _sender.Send(new ObtenerComprasPorProveedorQuery(proveedorId), ct);
        return FromResult(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear([FromBody] CrearCompraCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedFromResult(result, null, null!);
    }
}
