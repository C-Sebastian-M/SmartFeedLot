using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface ILoteSiloRepository
{
    Task<LoteSilo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LoteSilo>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LoteSilo>> ObtenerDisponiblesAsync(CancellationToken ct = default);
    Task AgregarAsync(LoteSilo lote, CancellationToken ct = default);
    void Eliminar(LoteSilo lote);
}
