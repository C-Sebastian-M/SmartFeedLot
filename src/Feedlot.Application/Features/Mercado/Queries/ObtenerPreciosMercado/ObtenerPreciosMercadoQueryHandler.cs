using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Queries.ObtenerPreciosMercado;

public sealed class ObtenerPreciosMercadoQueryHandler
    : IRequestHandler<ObtenerPreciosMercadoQuery, Result<IReadOnlyList<PrecioMercadoDto>>>
{
    private readonly IPrecioMercadoRepository _precioMercadoRepository;
    private readonly IMapper _mapper;

    public ObtenerPreciosMercadoQueryHandler(IPrecioMercadoRepository precioMercadoRepository, IMapper mapper)
    {
        _precioMercadoRepository = precioMercadoRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<PrecioMercadoDto>>> Handle(
        ObtenerPreciosMercadoQuery request, CancellationToken ct)
    {
        var precios = await _precioMercadoRepository.ObtenerTodosAsync(ct);
        var dtos = _mapper.Map<List<PrecioMercadoDto>>(precios);
        return Result<IReadOnlyList<PrecioMercadoDto>>.Success(dtos);
    }
}
