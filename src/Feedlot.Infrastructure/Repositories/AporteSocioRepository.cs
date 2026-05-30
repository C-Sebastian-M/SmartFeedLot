using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class AporteSocioRepository : IAporteSocioRepository
{
    private readonly FeedlotDbContext _context;

    public AporteSocioRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AporteSocio>> ObtenerPorItemAsync(Guid itemInversionId, CancellationToken ct = default)
        => await _context.Set<AporteSocio>()
            .Where(a => a.ItemInversionId == itemInversionId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AporteSocio>> ObtenerPorSocioAsync(Guid socioId, CancellationToken ct = default)
        => await _context.Set<AporteSocio>()
            .Where(a => a.SocioId == socioId)
            .ToListAsync(ct);

    public async Task AgregarAsync(AporteSocio aporte, CancellationToken ct = default)
        => await _context.Set<AporteSocio>().AddAsync(aporte, ct);

    public void Eliminar(AporteSocio aporte)
        => _context.Set<AporteSocio>().Remove(aporte);
}
