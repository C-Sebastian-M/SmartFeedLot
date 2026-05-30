using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface ISocioRepository
{
    Task<Socio?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Socio>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(Socio socio, CancellationToken ct = default);
    void Eliminar(Socio socio);
}
