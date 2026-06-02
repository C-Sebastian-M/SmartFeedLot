using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IPotreroRepository
{
    Task<Potrero?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);          // tracked  (para Retirar)
    Task<Potrero?> ObtenerPorIdSinTrackingAsync(Guid id, CancellationToken ct = default); // read-only (para Ingresar)
    Task<IReadOnlyList<Potrero>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(Potrero potrero, CancellationToken ct = default);
    void AgregarEstancia(EstanciaAnimal estancia);
    void Eliminar(Potrero potrero);
}
