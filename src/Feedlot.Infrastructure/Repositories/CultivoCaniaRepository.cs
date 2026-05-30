using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class CultivoCaniaRepository : ICultivoCaniaRepository
{
    private readonly FeedlotDbContext _context;
    public CultivoCaniaRepository(FeedlotDbContext context) => _context = context;

    public async Task<CultivoCania?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<CultivoCania>().Include(c => c.Cortes).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CultivoCania>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<CultivoCania>().Include(c => c.Cortes).OrderBy(c => c.Nombre).ToListAsync(ct);

    public async Task AgregarAsync(CultivoCania cultivo, CancellationToken ct = default)
        => await _context.Set<CultivoCania>().AddAsync(cultivo, ct);

    public void Eliminar(CultivoCania cultivo) => _context.Set<CultivoCania>().Remove(cultivo);
}
