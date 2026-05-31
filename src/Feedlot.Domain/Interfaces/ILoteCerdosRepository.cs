using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface ILoteCerdosRepository
{
    Task<LoteCerdos?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LoteCerdos>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(LoteCerdos lote, CancellationToken ct = default);
    void Eliminar(LoteCerdos lote);
}
