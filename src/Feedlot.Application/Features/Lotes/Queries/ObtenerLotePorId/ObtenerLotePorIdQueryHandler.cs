using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Queries.ObtenerLotePorId;

public sealed class ObtenerLotePorIdQueryHandler
    : IRequestHandler<ObtenerLotePorIdQuery, Result<LoteDto>>
{
    private readonly ILoteRepository _loteRepository;
    private readonly IMapper _mapper;

    public ObtenerLotePorIdQueryHandler(ILoteRepository loteRepository, IMapper mapper)
    {
        _loteRepository = loteRepository;
        _mapper = mapper;
    }

    public async Task<Result<LoteDto>> Handle(
        ObtenerLotePorIdQuery request,
        CancellationToken ct)
    {
        var lote = await _loteRepository.ObtenerPorIdAsync(request.LoteId, ct);

        if (lote is null)
            return Result<LoteDto>.NotFound(
                $"No se encontró el lote con ID '{request.LoteId}'.");

        var dto = _mapper.Map<LoteDto>(lote);
        return Result<LoteDto>.Success(dto);
    }
}
