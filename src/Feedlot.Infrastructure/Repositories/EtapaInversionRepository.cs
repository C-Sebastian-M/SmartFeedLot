using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class EtapaInversionRepository : IEtapaInversionRepository
{
    private readonly FeedlotDbContext _context;

    public EtapaInversionRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<EtapaInversion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<EtapaInversion>()
            .Include(e => e.Items)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<EtapaInversion?> ObtenerPorIdSinTrackingAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<EtapaInversion>()
            .AsNoTracking()
            .Include(e => e.Items)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<EtapaInversion>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<EtapaInversion>()
            .Include(e => e.Items)
            .OrderBy(e => e.Numero)
            .ToListAsync(ct);

    public async Task AgregarAsync(EtapaInversion etapa, CancellationToken ct = default)
        => await _context.Set<EtapaInversion>().AddAsync(etapa, ct);

    public void AgregarItem(ItemInversion item) => _context.Set<ItemInversion>().Add(item);

    public void Eliminar(EtapaInversion etapa)
        => _context.Set<EtapaInversion>().Remove(etapa);
}
