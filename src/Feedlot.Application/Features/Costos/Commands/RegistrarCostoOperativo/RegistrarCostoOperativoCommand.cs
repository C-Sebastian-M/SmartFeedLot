using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Costos.Commands.RegistrarCostoOperativo;

/// <summary>
/// Registra un costo de Mano de Obra o CIF en un lote.
/// Estos costos se prorratean entre los animales al calcular indicadores.
/// </summary>
public sealed record RegistrarCostoOperativoCommand(
    Guid LoteId,
    string Categoria,       // "ManoDeObra" | "CIF"
    string Concepto,
    DateOnly Fecha,
    decimal Monto,
    string Moneda,
    string? Observaciones,
    Guid RegistradoPorId
) : IRequest<Result<Guid>>;
