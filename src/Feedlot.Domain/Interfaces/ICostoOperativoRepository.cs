using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;

namespace Feedlot.Domain.Interfaces;

/// <summary>
/// Repositorio de CostoOperativo.
/// Permite consultar costos por lote, período y categoría para
/// el cálculo de costeo completo (MP + MO + CIF) por animal.
/// </summary>
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
    /// Suma el total de costos operativos de un lote en un período,
    /// opcionalmente filtrado por categoría.
    /// Usado por el servicio de costeo para calcular MO y CIF prorrateados.
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
