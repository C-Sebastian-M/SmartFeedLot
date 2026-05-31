using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class MarranaRepository : IMarranaRepository
{
    private readonly FeedlotDbContext _context;
    public MarranaRepository(FeedlotDbContext context) => _context = context;

    public async Task<Marrana?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Marrana>().Include(m => m.Camadas).FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Marrana>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<Marrana>().Include(m => m.Camadas).OrderBy(m => m.Identificacion).ToListAsync(ct);

    public async Task AgregarAsync(Marrana marrana, CancellationToken ct = default)
        => await _context.Set<Marrana>().AddAsync(marrana, ct);

    public void Eliminar(Marrana marrana) => _context.Set<Marrana>().Remove(marrana);
}
