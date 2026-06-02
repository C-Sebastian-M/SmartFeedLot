using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class EmpleadoRepository : IEmpleadoRepository
{
    private readonly FeedlotDbContext _context;
    public EmpleadoRepository(FeedlotDbContext context) => _context = context;

    public async Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Empleado>().Include(e => e.Actividades).FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Empleado>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<Empleado>().Include(e => e.Actividades).OrderBy(e => e.Nombre).ToListAsync(ct);

    public async Task AgregarAsync(Empleado empleado, CancellationToken ct = default)
        => await _context.Set<Empleado>().AddAsync(empleado, ct);

    public async Task<Empleado?> ObtenerPorIdSinTrackingAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Empleado>().AsNoTracking().Include(e => e.Actividades).FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<ActividadManoObra?> ObtenerActividadPorIdAsync(Guid actividadId, CancellationToken ct = default)
        => await _context.Set<ActividadManoObra>().FirstOrDefaultAsync(a => a.Id == actividadId, ct);

    public void AgregarActividad(ActividadManoObra actividad) => _context.Set<ActividadManoObra>().Add(actividad);

    public void Actualizar(Empleado empleado) => _context.Set<Empleado>().Update(empleado);
    public void Eliminar(Empleado empleado) => _context.Set<Empleado>().Remove(empleado);
}
