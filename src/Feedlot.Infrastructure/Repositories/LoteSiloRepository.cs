using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class LoteSiloRepository : ILoteSiloRepository
{
    private readonly FeedlotDbContext _context;
    public LoteSiloRepository(FeedlotDbContext context) => _context = context;

    public async Task<LoteSilo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<LoteSilo>().FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<LoteSilo>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<LoteSilo>().OrderByDescending(l => l.FechaProduccion).ToListAsync(ct);

    public async Task<IReadOnlyList<LoteSilo>> ObtenerDisponiblesAsync(CancellationToken ct = default)
        => await _context.Set<LoteSilo>().Where(l => l.Bolsas > 0).OrderBy(l => l.FechaProduccion).ToListAsync(ct);

    public async Task AgregarAsync(LoteSilo lote, CancellationToken ct = default)
        => await _context.Set<LoteSilo>().AddAsync(lote, ct);

    public void Eliminar(LoteSilo lote) => _context.Set<LoteSilo>().Remove(lote);
}
