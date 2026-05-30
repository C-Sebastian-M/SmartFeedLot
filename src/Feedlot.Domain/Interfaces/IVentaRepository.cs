using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IVentaRepository
{
    Task<Venta?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Venta>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Venta>> ObtenerPorPeriodoAsync(int anio, int? mes = null, CancellationToken ct = default);
    Task AgregarAsync(Venta venta, CancellationToken ct = default);
}
