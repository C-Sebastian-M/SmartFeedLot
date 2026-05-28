using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Animals.Queries.ObtenerAnimales;

public sealed class ObtenerAnimalesQueryHandler
    : IRequestHandler<ObtenerAnimalesQuery, Result<PagedResult<AnimalResumenDto>>>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly IMapper _mapper;

    public ObtenerAnimalesQueryHandler(IAnimalRepository animalRepository, IMapper mapper)
    {
        _animalRepository = animalRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AnimalResumenDto>>> Handle(
        ObtenerAnimalesQuery request,
        CancellationToken ct)
    {
        EstadoProductivo? ep = null;
        if (!string.IsNullOrWhiteSpace(request.EstadoProductivo) &&
            Enum.TryParse<EstadoProductivo>(request.EstadoProductivo, true, out var parsedEp))
            ep = parsedEp;

        EstadoSanitario? es = null;
        if (!string.IsNullOrWhiteSpace(request.EstadoSanitario) &&
            Enum.TryParse<EstadoSanitario>(request.EstadoSanitario, true, out var parsedEs))
            es = parsedEs;

        var (items, totalCount) = await _animalRepository.ObtenerPaginadosAsync(
            request.Page,
            request.PageSize,
            ep,
            es,
            request.Raza,
            request.Busqueda,
            ct);

        var dtos = _mapper.Map<List<AnimalResumenDto>>(items);

        return Result<PagedResult<AnimalResumenDto>>.Success(
            PagedResult<AnimalResumenDto>.Create(dtos, totalCount, request.Page, request.PageSize));
    }
}
