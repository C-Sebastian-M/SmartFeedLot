using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class SubaganEventoRepository : ISubaganEventoRepository
{
    private readonly FeedlotDbContext _context;
    public SubaganEventoRepository(FeedlotDbContext context) => _context = context;

    public async Task<bool> ExisteAsync(int subaganEventoId, CancellationToken ct = default)
        => await _context.Set<SubaganEvento>().AnyAsync(e => e.SubaganEventoId == subaganEventoId, ct);

    public async Task<IReadOnlyList<SubaganEvento>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<SubaganEvento>()
            .OrderByDescending(e => e.Fecha)
            .ToListAsync(ct);

    public async Task<SubaganEvento?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<SubaganEvento>()
            .Include(e => e.Lotes)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<SubaganLote>> ObtenerLotesPorEventoAsync(Guid eventoId, CancellationToken ct = default)
        => await _context.Set<SubaganLote>()
            .Where(l => l.SubaganEventoId == eventoId)
            .OrderBy(l => l.NumeroLote)
            .ToListAsync(ct);

    public async Task AgregarAsync(SubaganEvento evento, CancellationToken ct = default)
        => await _context.Set<SubaganEvento>().AddAsync(evento, ct);

    public void Eliminar(SubaganEvento evento)
        => _context.Set<SubaganEvento>().Remove(evento);
}
