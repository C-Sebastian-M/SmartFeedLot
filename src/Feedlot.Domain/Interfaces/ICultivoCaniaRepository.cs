using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface ICultivoCaniaRepository
{
    Task<CultivoCania?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<CultivoCania?> ObtenerPorIdSinTrackingAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CultivoCania>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(CultivoCania cultivo, CancellationToken ct = default);
    void AgregarCorte(CorteCania corte);
    void Eliminar(CultivoCania cultivo);
}
