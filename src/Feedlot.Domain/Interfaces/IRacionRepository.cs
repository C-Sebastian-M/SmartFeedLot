using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

/// <summary>Contrato del repositorio de Racion.</summary>
public interface IRacionRepository
{
    Task<Racion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Racion>> ObtenerActivasAsync(CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default);
    Task AgregarAsync(Racion racion, CancellationToken ct = default);
    void Actualizar(Racion racion);
}
