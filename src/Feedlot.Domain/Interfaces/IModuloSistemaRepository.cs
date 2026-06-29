using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IModuloSistemaRepository
{
    Task<IReadOnlyList<ModuloSistema>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<ModuloSistema?> ObtenerPorClaveAsync(string clave, CancellationToken ct = default);
}
