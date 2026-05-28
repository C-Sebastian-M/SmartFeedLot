using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Ventas.Queries.ObtenerCompradores;

public sealed class ObtenerCompradoresQueryHandler
    : IRequestHandler<ObtenerCompradoresQuery, Result<IReadOnlyList<CompradorDto>>>
{
    private readonly ICompradorRepository _compradorRepository;
    private readonly IMapper _mapper;

    public ObtenerCompradoresQueryHandler(ICompradorRepository compradorRepository, IMapper mapper)
    {
        _compradorRepository = compradorRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<CompradorDto>>> Handle(
        ObtenerCompradoresQuery request, CancellationToken ct)
    {
        var compradores = await _compradorRepository.ObtenerTodosAsync(ct);
        var dtos = _mapper.Map<List<CompradorDto>>(compradores);
        return Result<IReadOnlyList<CompradorDto>>.Success(dtos);
    }
}
