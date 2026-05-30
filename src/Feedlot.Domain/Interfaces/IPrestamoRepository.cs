using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IPrestamoRepository
{
    Task<Prestamo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Prestamo>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(Prestamo prestamo, CancellationToken ct = default);
    void Eliminar(Prestamo prestamo);
}
