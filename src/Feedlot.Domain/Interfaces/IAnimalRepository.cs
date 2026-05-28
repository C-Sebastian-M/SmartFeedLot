using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;

namespace Feedlot.Domain.Interfaces;

public interface IAnimalRepository
{
    Task<Animal?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Animal?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Animal>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default);
    Task<(IReadOnlyList<Animal> Items, int TotalCount)> ObtenerPaginadosAsync(
        int page,
        int pageSize,
        EstadoProductivo? estadoProductivo,
        EstadoSanitario? estadoSanitario,
        string? raza,
        string? busqueda,
        CancellationToken ct = default);
    Task<string> ObtenerSiguienteCodigoAsync(CancellationToken ct = default);
    Task<string> ObtenerSiguienteAreteAsync(CancellationToken ct = default);
    Task<Dictionary<Guid, (string Codigo, string? Nombre)>> ObtenerCodigosPorIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default);
    Task AgregarAsync(Animal animal, CancellationToken ct = default);
    void Actualizar(Animal animal);
    Task EliminarAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<(Guid AnimalId, string Codigo, string? Nombre, string Diagnostico, DateOnly ProximaDosis, string? Responsable)>>
        ObtenerVacunasProximasAsync(int dias, CancellationToken ct = default);
}
