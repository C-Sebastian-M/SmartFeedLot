using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Compras.Queries.ObtenerCompras;

public sealed class ObtenerComprasQueryHandler
    : IRequestHandler<ObtenerComprasQuery, Result<IReadOnlyList<CompraDto>>>
{
    private readonly ICompraRepository _compraRepository;
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;

    public ObtenerComprasQueryHandler(
        ICompraRepository compraRepository,
        IProveedorRepository proveedorRepository,
        IMapper mapper)
    {
        _compraRepository = compraRepository;
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<CompraDto>>> Handle(
        ObtenerComprasQuery request, CancellationToken ct)
    {
        var compras = await _compraRepository.ObtenerTodosAsync(ct);
        var proveedorIds = compras.Select(c => c.ProveedorId).Distinct().ToList();

        var proveedores = new Dictionary<Guid, string>();
        foreach (var pid in proveedorIds)
        {
            var p = await _proveedorRepository.ObtenerPorIdAsync(pid, ct);
            if (p is not null)
                proveedores[pid] = p.Nombre;
        }

        var dtos = _mapper.Map<List<CompraDto>>(compras);
        foreach (var dto in dtos)
        {
            if (proveedores.TryGetValue(dto.ProveedorId, out var nombre))
                dto.NombreProveedor = nombre;
        }

        return Result<IReadOnlyList<CompraDto>>.Success(dtos);
    }
}
