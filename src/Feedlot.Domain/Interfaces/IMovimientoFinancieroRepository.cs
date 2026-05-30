using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;

namespace Feedlot.Domain.Interfaces;

public interface IMovimientoFinancieroRepository
{
    Task<MovimientoFinanciero?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    
    Task<IReadOnlyList<MovimientoFinanciero>> ObtenerPorFiltroAsync(
        int? anio = null,
        int? mes = null,
        OrigenFinanciero? origen = null,
        Guid? categoriaGastoId = null,
        Guid? socioId = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<MovimientoFinanciero>> ObtenerPorRangoFechasAsync(
        DateOnly desde,
        DateOnly hasta,
        OrigenFinanciero? origen = null,
        CancellationToken ct = default);

    Task AgregarAsync(MovimientoFinanciero movimiento, CancellationToken ct = default);
    void Actualizar(MovimientoFinanciero movimiento);
    void Eliminar(MovimientoFinanciero movimiento);
}
