using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface ISubaganEventoRepository
{
    Task<bool> ExisteAsync(int subaganEventoId, CancellationToken ct = default);
    Task<IReadOnlyList<SubaganEvento>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<SubaganEvento?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SubaganLote>> ObtenerLotesPorEventoAsync(Guid eventoId, CancellationToken ct = default);
    Task AgregarAsync(SubaganEvento evento, CancellationToken ct = default);
    void Eliminar(SubaganEvento evento);
}
