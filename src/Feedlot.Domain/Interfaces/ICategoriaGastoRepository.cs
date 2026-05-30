using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface ICategoriaGastoRepository
{
    Task<CategoriaGasto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<CategoriaGasto?> ObtenerPorNombreAsync(string nombre, CancellationToken ct = default);
    Task<IReadOnlyList<CategoriaGasto>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(CategoriaGasto categoria, CancellationToken ct = default);
    void Eliminar(CategoriaGasto categoria);
}
