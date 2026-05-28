using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Analitica.Queries.ObtenerVacunasProximas;

public sealed class ObtenerVacunasProximasQueryHandler
    : IRequestHandler<ObtenerVacunasProximasQuery, Result<IReadOnlyList<VacunasProximasDto>>>
{
    private readonly IAnimalRepository _animalRepository;

    public ObtenerVacunasProximasQueryHandler(IAnimalRepository animalRepository)
    {
        _animalRepository = animalRepository;
    }

    public async Task<Result<IReadOnlyList<VacunasProximasDto>>> Handle(
        ObtenerVacunasProximasQuery request,
        CancellationToken ct)
    {
        var data = await _animalRepository.ObtenerVacunasProximasAsync(request.Dias, ct);

        var dto = data
            .Select(d => new VacunasProximasDto(
                d.AnimalId, d.Codigo, d.Nombre, d.Diagnostico, d.ProximaDosis, d.Responsable))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<VacunasProximasDto>>.Success(dto);
    }
}