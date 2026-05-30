using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IEmpleadoRepository
{
    Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Empleado>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(Empleado empleado, CancellationToken ct = default);
    void Eliminar(Empleado empleado);
}
