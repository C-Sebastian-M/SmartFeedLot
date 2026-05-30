using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IPotreroRepository
{
    Task<Potrero?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Potrero>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(Potrero potrero, CancellationToken ct = default);
    void Eliminar(Potrero potrero);
}
