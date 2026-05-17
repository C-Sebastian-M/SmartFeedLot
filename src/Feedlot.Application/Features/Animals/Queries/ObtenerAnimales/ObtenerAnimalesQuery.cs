using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Animals.Queries.ObtenerAnimales;

/// <summary>
/// Query para obtener la lista paginada de animales con filtros opcionales.
/// En CQRS, las Queries solo leen — nunca modifican estado.
/// </summary>
public sealed record ObtenerAnimalesQuery(
    int Page = 1,
    int PageSize = 20,
    string? EstadoProductivo = null,
    string? EstadoSanitario = null,
    string? Raza = null,
    string? Busqueda = null
) : IRequest<Result<PagedResult<AnimalResumenDto>>>;
