using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

/// <summary>Contrato del repositorio de Ingrediente.</summary>
public interface IIngredienteRepository
{
    Task<Ingrediente?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Ingrediente>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default);
    Task AgregarAsync(Ingrediente ingrediente, CancellationToken ct = default);
    void Actualizar(Ingrediente ingrediente);
}
