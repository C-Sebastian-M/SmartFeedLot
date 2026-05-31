using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Commands.CrearPrecioMercado;

public sealed class CrearPrecioMercadoCommandHandler
    : IRequestHandler<CrearPrecioMercadoCommand, Result<Guid>>
{
    private readonly IPrecioMercadoRepository _precioMercadoRepository;

    public CrearPrecioMercadoCommandHandler(IPrecioMercadoRepository precioMercadoRepository)
    {
        _precioMercadoRepository = precioMercadoRepository;
    }

    public async Task<Result<Guid>> Handle(CrearPrecioMercadoCommand request, CancellationToken ct)
    {
        var precio = PrecioMercado.Crear(request.Fecha, request.Especie, request.Tipo, request.PrecioPorKg, request.Fuente);
        await _precioMercadoRepository.AgregarAsync(precio, ct);
        return Result<Guid>.Success(precio.Id);
    }
}
