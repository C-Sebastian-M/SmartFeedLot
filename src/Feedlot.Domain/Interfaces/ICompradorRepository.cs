using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface ICompradorRepository
{
    Task<Comprador?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Comprador>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(Comprador comprador, CancellationToken ct = default);
    void Actualizar(Comprador comprador);
    Task<bool> EliminarAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteConNombreAsync(string nombre, Guid? excludeId = null, CancellationToken ct = default);
}
