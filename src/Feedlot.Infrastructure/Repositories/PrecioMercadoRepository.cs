using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class PrecioMercadoRepository : IPrecioMercadoRepository
{
    private readonly FeedlotDbContext _context;
    public PrecioMercadoRepository(FeedlotDbContext context) => _context = context;

    public async Task<PrecioMercado?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<PrecioMercado>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<PrecioMercado>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<PrecioMercado>().OrderByDescending(p => p.Fecha).ToListAsync(ct);

    public async Task AgregarAsync(PrecioMercado precio, CancellationToken ct = default)
        => await _context.Set<PrecioMercado>().AddAsync(precio, ct);

    public void Eliminar(PrecioMercado precio) => _context.Set<PrecioMercado>().Remove(precio);
}
