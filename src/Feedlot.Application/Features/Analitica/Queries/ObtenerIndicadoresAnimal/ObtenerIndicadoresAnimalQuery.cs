using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Analitica.Queries.ObtenerIndicadoresAnimal;

/// <summary>
/// Query que calcula todos los indicadores productivos de un animal
/// en un período específico. Es la query más importante del sistema —
/// el núcleo analítico del feedlot.
/// </summary>
public sealed record ObtenerIndicadoresAnimalQuery(
    Guid AnimalId,
    Guid LoteId,
    DateOnly Desde,
    DateOnly Hasta,
    decimal PrecioVentaEstimadoPorKg
) : IRequest<Result<IndicadorProductivoDto>>;
