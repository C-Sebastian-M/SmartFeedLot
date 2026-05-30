using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class PotreroRepository : IPotreroRepository
{
    private readonly FeedlotDbContext _context;
    public PotreroRepository(FeedlotDbContext context) => _context = context;

    public async Task<Potrero?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Potrero>().Include(p => p.Estancias).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Potrero>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<Potrero>().Include(p => p.Estancias).OrderBy(p => p.Nombre).ToListAsync(ct);

    public async Task AgregarAsync(Potrero potrero, CancellationToken ct = default)
        => await _context.Set<Potrero>().AddAsync(potrero, ct);

    public void Eliminar(Potrero potrero) => _context.Set<Potrero>().Remove(potrero);
}
