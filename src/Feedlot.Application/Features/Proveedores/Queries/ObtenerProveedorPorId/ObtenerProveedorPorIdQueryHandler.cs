using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Proveedores.Queries.ObtenerProveedorPorId;

public sealed class ObtenerProveedorPorIdQueryHandler
    : IRequestHandler<ObtenerProveedorPorIdQuery, Result<ProveedorDto>>
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;

    public ObtenerProveedorPorIdQueryHandler(IProveedorRepository proveedorRepository, IMapper mapper)
    {
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
    }

    public async Task<Result<ProveedorDto>> Handle(
        ObtenerProveedorPorIdQuery request, CancellationToken ct)
    {
        var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.Id, ct);
        if (proveedor is null)
            return Result<ProveedorDto>.NotFound($"Proveedor {request.Id} no encontrado.");

        var dto = _mapper.Map<ProveedorDto>(proveedor);
        return Result<ProveedorDto>.Success(dto);
    }
}
