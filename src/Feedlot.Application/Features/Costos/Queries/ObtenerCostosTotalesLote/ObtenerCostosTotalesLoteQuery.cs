using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Costos.Queries.ObtenerCostosTotalesLote;

public sealed record ObtenerCostosTotalesLoteQuery(
    Guid LoteId,
    DateOnly Desde,
    DateOnly Hasta
) : IRequest<Result<CosteoLoteDto>>;

/// <summary>
/// DTO con el desglose completo de costos del lote,
/// equivalente a la estructura de costeo del Excel.
/// </summary>
public sealed class CosteoLoteDto
{
    public Guid LoteId { get; init; }
    public string CodigoLote { get; init; } = null!;
    public int TotalAnimales { get; init; }
    public DateOnly Desde { get; init; }
    public DateOnly Hasta { get; init; }

    // ── Materia prima (alimento) ──────────────────────────────────────────────
    public decimal CostoTotalAlimento { get; init; }
    public decimal CostoAlimentoPorAnimal { get; init; }
    public decimal ConsumoTotalKg { get; init; }

    // ── Mano de obra ─────────────────────────────────────────────────────────
    public decimal CostoTotalManoDeObra { get; init; }
    public decimal CostoManoDeObraPorAnimal { get; init; }
    public IReadOnlyList<CostoDetalleDto> DetallesManoDeObra { get; init; } = [];

    // ── CIF ───────────────────────────────────────────────────────────────────
    public decimal CostoTotalCif { get; init; }
    public decimal CostoCifPorAnimal { get; init; }
    public IReadOnlyList<CostoDetalleDto> DetallesCif { get; init; } = [];

    // ── Totales ───────────────────────────────────────────────────────────────
    /// <summary>
    /// Costo total del lote = Alimento + MO + CIF.
    /// No incluye precio de compra de animales (varía por animal).
    /// </summary>
    public decimal CostoOperativoTotal { get; init; }

    /// <summary>Costo operativo prorrateado por animal.</summary>
    public decimal CostoOperativoPorAnimal { get; init; }
}

public sealed class CostoDetalleDto
{
    public Guid Id { get; init; }
    public string Categoria { get; init; } = null!;
    public string Concepto { get; init; } = null!;
    public DateOnly Fecha { get; init; }
    public decimal Monto { get; init; }
    public string Moneda { get; init; } = null!;
    public string? Observaciones { get; init; }
}
