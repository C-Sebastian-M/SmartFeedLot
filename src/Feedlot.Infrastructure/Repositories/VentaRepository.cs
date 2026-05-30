using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class VentaRepository : IVentaRepository
{
    private readonly FeedlotDbContext _context;

    public VentaRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Venta?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Venta>()
            .Include(v => v.Items)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IReadOnlyList<Venta>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<Venta>()
            .Include(v => v.Items)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Venta>> ObtenerPorPeriodoAsync(int anio, int? mes = null, CancellationToken ct = default)
    {
        var query = _context.Set<Venta>()
            .Include(v => v.Items)
            .Where(v => v.Fecha.Year == anio);

        if (mes.HasValue)
            query = query.Where(v => v.Fecha.Month == mes.Value);

        return await query.OrderByDescending(v => v.Fecha).ToListAsync(ct);
    }

    public async Task AgregarAsync(Venta venta, CancellationToken ct = default)
        => await _context.Set<Venta>().AddAsync(venta, ct);
}
