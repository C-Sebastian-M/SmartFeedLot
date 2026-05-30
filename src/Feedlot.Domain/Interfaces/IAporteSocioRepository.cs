using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IAporteSocioRepository
{
    Task<IReadOnlyList<AporteSocio>> ObtenerPorItemAsync(Guid itemInversionId, CancellationToken ct = default);
    Task<IReadOnlyList<AporteSocio>> ObtenerPorSocioAsync(Guid socioId, CancellationToken ct = default);
    Task AgregarAsync(AporteSocio aporte, CancellationToken ct = default);
    void Eliminar(AporteSocio aporte);
}
