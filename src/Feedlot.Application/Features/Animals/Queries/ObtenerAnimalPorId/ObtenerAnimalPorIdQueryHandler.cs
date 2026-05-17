using AutoMapper;
using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Animals.Queries.ObtenerAnimalPorId;

public sealed class ObtenerAnimalPorIdQueryHandler
    : IRequestHandler<ObtenerAnimalPorIdQuery, Result<AnimalDto>>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly IMapper _mapper;

    public ObtenerAnimalPorIdQueryHandler(IAnimalRepository animalRepository, IMapper mapper)
    {
        _animalRepository = animalRepository;
        _mapper = mapper;
    }

    public async Task<Result<AnimalDto>> Handle(
        ObtenerAnimalPorIdQuery request,
        CancellationToken ct)
    {
        var animal = await _animalRepository.ObtenerPorIdAsync(request.AnimalId, ct);

        if (animal is null)
            return Result<AnimalDto>.NotFound(
                $"No se encontró el animal con ID '{request.AnimalId}'.");

        var dto = _mapper.Map<AnimalDto>(animal);
        return Result<AnimalDto>.Success(dto);
    }
}
