using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Queries.ObtenerLotes;

public sealed class ObtenerLotesQueryHandler
    : IRequestHandler<ObtenerLotesQuery, Result<IReadOnlyList<LoteResumenDto>>>
{
    private readonly ILoteRepository _loteRepository;
    private readonly IMapper _mapper;

    public ObtenerLotesQueryHandler(ILoteRepository loteRepository, IMapper mapper)
    {
        _loteRepository = loteRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<LoteResumenDto>>> Handle(
        ObtenerLotesQuery request,
        CancellationToken ct)
    {
        var lotes = request.SoloActivos
            ? await _loteRepository.ObtenerActivosAsync(ct)
            : await _loteRepository.ObtenerTodosAsync(ct);

        var dtos = _mapper.Map<IReadOnlyList<LoteResumenDto>>(lotes);
        return Result<IReadOnlyList<LoteResumenDto>>.Success(dtos);
    }
}
