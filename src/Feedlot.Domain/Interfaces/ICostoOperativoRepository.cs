using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;

namespace Feedlot.Domain.Interfaces;

public interface ICostoOperativoRepository
{
    Task<CostoOperativo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CostoOperativo>> ObtenerPorLoteAsync(
        Guid loteId,
        DateOnly? desde = null,
        DateOnly? hasta = null,
        CategoriaCosto? categoria = null,
        CancellationToken ct = default);

    /// <summary>
    /// Suma el total de costos operativos de un lote en un período usando SUM()
    /// directamente en PostgreSQL. Opcionalmente filtrado por categoría (MO o CIF).
    /// </summary>
    Task<decimal> SumarMontoPorLoteAsync(
        Guid loteId,
        DateOnly desde,
        DateOnly hasta,
        CategoriaCosto? categoria = null,
        CancellationToken ct = default);

    Task AgregarAsync(CostoOperativo costo, CancellationToken ct = default);
    void Actualizar(CostoOperativo costo);
}
