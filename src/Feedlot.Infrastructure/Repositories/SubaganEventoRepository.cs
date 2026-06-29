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

    public async Task<IReadOnlyDictionary<string, decimal>> ObtenerPreciosPorTipoAsync(
        Guid eventoId, CancellationToken ct = default)
    {
        // Traemos los lotes del evento y calculamos en memoria el promedio
        // ponderado por cantidad de animales para cada código de tipo.
        var lotes = await _context.Set<SubaganLote>()
            .Where(l => l.SubaganEventoId == eventoId && l.PrecioPorKg > 0 && l.Cantidad > 0)
            .Select(l => new { l.CodigoTipo, l.PrecioPorKg, l.Cantidad })
            .ToListAsync(ct);

        return lotes
            .GroupBy(l => l.CodigoTipo.Trim().ToUpperInvariant())
            .ToDictionary(
                g => g.Key,
                g => Math.Round(
                    g.Sum(x => x.PrecioPorKg * x.Cantidad) / g.Sum(x => x.Cantidad), 2));
    }
}
