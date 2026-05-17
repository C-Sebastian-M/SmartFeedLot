using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Analitica.Queries.ObtenerResumenLote;

/// <summary>
/// Query que produce el resumen ejecutivo de un lote completo:
/// agrega indicadores de todos sus animales, consumo total y rentabilidad proyectada.
/// Es la query que alimenta el dashboard principal del feedlot.
/// </summary>
public sealed record ObtenerResumenLoteQuery(
    Guid LoteId,
    DateOnly Desde,
    DateOnly Hasta,
    decimal PrecioVentaEstimadoPorKg
) : IRequest<Result<ResumenLoteDto>>;
