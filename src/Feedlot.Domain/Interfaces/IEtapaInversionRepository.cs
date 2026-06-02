using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IEtapaInversionRepository
{
    Task<EtapaInversion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<EtapaInversion?> ObtenerPorIdSinTrackingAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<EtapaInversion>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(EtapaInversion etapa, CancellationToken ct = default);
    void AgregarItem(ItemInversion item);
    void Eliminar(EtapaInversion etapa);
}
