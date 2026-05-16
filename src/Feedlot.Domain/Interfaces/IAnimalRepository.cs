using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

/// <summary>
/// Contrato del repositorio de Animal. Definido en Domain, implementado en Infrastructure.
/// Principio de Inversión de Dependencias: Domain no depende de Infrastructure.
/// </summary>
public interface IAnimalRepository
{
    Task<Animal?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Animal?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Animal>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default);
    Task AgregarAsync(Animal animal, CancellationToken ct = default);
    void Actualizar(Animal animal);
}
