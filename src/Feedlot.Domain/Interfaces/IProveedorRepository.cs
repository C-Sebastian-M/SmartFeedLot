using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IProveedorRepository
{
    Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Proveedor>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(Proveedor proveedor, CancellationToken ct = default);
    void Actualizar(Proveedor proveedor);
    Task<bool> EliminarAsync(Guid id, CancellationToken ct = default);
}