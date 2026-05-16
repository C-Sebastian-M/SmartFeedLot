using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

/// <summary>Contrato del repositorio de ConsumoAlimenticio.</summary>
public interface IConsumoAlimenticioRepository
{
    Task<ConsumoAlimenticio?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ConsumoAlimenticio>> ObtenerPorLoteAsync(
        Guid loteId,
        DateOnly? desde = null,
        DateOnly? hasta = null,
        CancellationToken ct = default);

    /// <summary>
    /// Retorna el total de kilogramos consumidos por un lote en un período.
    /// Usado por el Domain Service de Analítica para calcular ICA.
    /// </summary>
    Task<decimal> SumarKilogramosPorLoteAsync(
        Guid loteId,
        DateOnly desde,
        DateOnly hasta,
        CancellationToken ct = default);

    /// <summary>
    /// Retorna el costo total de alimento consumido por un lote en un período.
    /// Usado para calcular costo por kg ganado y rentabilidad.
    /// </summary>
    Task<decimal> SumarCostoPorLoteAsync(
        Guid loteId,
        DateOnly desde,
        DateOnly hasta,
        CancellationToken ct = default);

    Task AgregarAsync(ConsumoAlimenticio consumo, CancellationToken ct = default);
    void Actualizar(ConsumoAlimenticio consumo);
}
