using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IEmpleadoRepository
{
    Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Empleado>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(Empleado empleado, CancellationToken ct = default);
    Task<Empleado?> ObtenerPorIdSinTrackingAsync(Guid id, CancellationToken ct = default);
    Task<ActividadManoObra?> ObtenerActividadPorIdAsync(Guid actividadId, CancellationToken ct = default);
    void AgregarActividad(ActividadManoObra actividad);
    void Actualizar(Empleado empleado);
    void Eliminar(Empleado empleado);
}
