using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IMarranaRepository
{
    Task<Marrana?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Marrana>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(Marrana marrana, CancellationToken ct = default);
    void Eliminar(Marrana marrana);
}
