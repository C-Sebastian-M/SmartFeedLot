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
        var todos = await _animalRepository.ObtenerTodosAsync(ct);

        // Filtros en memoria — en fase 3 (Infrastructure) se optimizarán como
        // IQueryable con EF Core para moverlos a la base de datos.
        var filtrados = todos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.EstadoProductivo) &&
            Enum.TryParse<EstadoProductivo>(request.EstadoProductivo, true, out var ep))
            filtrados = filtrados.Where(a => a.EstadoProductivo == ep);

        if (!string.IsNullOrWhiteSpace(request.EstadoSanitario) &&
            Enum.TryParse<EstadoSanitario>(request.EstadoSanitario, true, out var es))
            filtrados = filtrados.Where(a => a.EstadoSanitario == es);

        if (!string.IsNullOrWhiteSpace(request.Raza))
            filtrados = filtrados.Where(a =>
                a.Raza.Contains(request.Raza, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.Busqueda))
            filtrados = filtrados.Where(a =>
                a.CodigoIdentificacion.Valor.Contains(
                    request.Busqueda, StringComparison.OrdinalIgnoreCase) ||
                a.NumeroArete.Contains(
                    request.Busqueda, StringComparison.OrdinalIgnoreCase));

        var lista = filtrados.ToList();
        var totalCount = lista.Count;

        var paginados = lista
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<AnimalResumenDto>>(paginados);

        return Result<PagedResult<AnimalResumenDto>>.Success(
            PagedResult<AnimalResumenDto>.Create(dtos, totalCount, request.Page, request.PageSize));
    }
}
