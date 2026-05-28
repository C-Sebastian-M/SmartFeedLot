using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Proveedores.Queries.ObtenerProveedores;

public sealed class ObtenerProveedoresQueryHandler
    : IRequestHandler<ObtenerProveedoresQuery, Result<IReadOnlyList<ProveedorDto>>>
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;

    public ObtenerProveedoresQueryHandler(IProveedorRepository proveedorRepository, IMapper mapper)
    {
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<ProveedorDto>>> Handle(
        ObtenerProveedoresQuery request, CancellationToken ct)
    {
        var proveedores = await _proveedorRepository.ObtenerTodosAsync(ct);
        var dtos = _mapper.Map<List<ProveedorDto>>(proveedores);
        return Result<IReadOnlyList<ProveedorDto>>.Success(dtos);
    }
}
