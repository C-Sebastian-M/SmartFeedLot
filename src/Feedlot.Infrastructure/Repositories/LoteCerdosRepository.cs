using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class LoteCerdosRepository : ILoteCerdosRepository
{
    private readonly FeedlotDbContext _context;
    public LoteCerdosRepository(FeedlotDbContext context) => _context = context;

    public async Task<LoteCerdos?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<LoteCerdos>().FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<LoteCerdos>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<LoteCerdos>().OrderBy(l => l.FechaInicio).ToListAsync(ct);

    public async Task AgregarAsync(LoteCerdos lote, CancellationToken ct = default)
        => await _context.Set<LoteCerdos>().AddAsync(lote, ct);

    public void Eliminar(LoteCerdos lote) => _context.Set<LoteCerdos>().Remove(lote);
}
