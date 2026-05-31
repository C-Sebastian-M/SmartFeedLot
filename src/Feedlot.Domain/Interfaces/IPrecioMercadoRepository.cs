using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IPrecioMercadoRepository
{
    Task<PrecioMercado?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PrecioMercado>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(PrecioMercado precio, CancellationToken ct = default);
    void Eliminar(PrecioMercado precio);
}
