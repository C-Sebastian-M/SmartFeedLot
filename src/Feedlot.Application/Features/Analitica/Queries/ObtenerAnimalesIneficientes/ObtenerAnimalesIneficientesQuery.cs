using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using MediatR;

namespace Feedlot.Application.Features.Analitica.Queries.ObtenerAnimalesIneficientes;

/// <summary>
/// Query que detecta animales por debajo de los umbrales productivos mínimos.
/// Alimenta el panel de alertas del dashboard ejecutivo.
/// </summary>
public sealed record ObtenerAnimalesIneficientesQuery(
    Guid? LoteId,
    DateOnly Desde,
    DateOnly Hasta,
    decimal PrecioVentaEstimadoPorKg,
    decimal GmdMinimaKgDia = 0.8m,
    decimal IcaMaxima = 8.0m
) : IRequest<Result<IReadOnlyList<AnimalIneficienteDto>>>;
