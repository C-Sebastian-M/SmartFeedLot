using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface ICompraRepository
{
    Task<Compra?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Compra>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Compra>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken ct = default);
    Task AgregarAsync(Compra compra, CancellationToken ct = default);
}