using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Compras.Queries.ObtenerComprasPorProveedor;

public sealed class ObtenerComprasPorProveedorQueryHandler
    : IRequestHandler<ObtenerComprasPorProveedorQuery, Result<IReadOnlyList<CompraDto>>>
{
    private readonly ICompraRepository _compraRepository;
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;

    public ObtenerComprasPorProveedorQueryHandler(
        ICompraRepository compraRepository,
        IProveedorRepository proveedorRepository,
        IMapper mapper)
    {
        _compraRepository = compraRepository;
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<CompraDto>>> Handle(
        ObtenerComprasPorProveedorQuery request, CancellationToken ct)
    {
        var compras = await _compraRepository.ObtenerPorProveedorAsync(request.ProveedorId, ct);
        var dtos = _mapper.Map<List<CompraDto>>(compras);

        var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.ProveedorId, ct);
        var nombreProveedor = proveedor?.Nombre ?? "Desconocido";

        foreach (var dto in dtos)
            dto.NombreProveedor = nombreProveedor;

        return Result<IReadOnlyList<CompraDto>>.Success(dtos);
    }
}
