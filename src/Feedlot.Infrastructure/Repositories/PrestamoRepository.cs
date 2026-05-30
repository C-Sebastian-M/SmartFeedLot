using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class PrestamoRepository : IPrestamoRepository
{
    private readonly FeedlotDbContext _context;

    public PrestamoRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Prestamo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Prestamo>()
            .Include(p => p.Cuotas)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Prestamo>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<Prestamo>()
            .Include(p => p.Cuotas)
            .OrderByDescending(p => p.FechaInicio)
            .ToListAsync(ct);

    public async Task AgregarAsync(Prestamo prestamo, CancellationToken ct = default)
        => await _context.Set<Prestamo>().AddAsync(prestamo, ct);

    public void Eliminar(Prestamo prestamo)
        => _context.Set<Prestamo>().Remove(prestamo);
}
