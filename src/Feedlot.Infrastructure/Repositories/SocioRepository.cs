using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class SocioRepository : ISocioRepository
{
    private readonly FeedlotDbContext _context;

    public SocioRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Socio?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Socio>().FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<Socio>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<Socio>().OrderBy(s => s.Nombre).ToListAsync(ct);

    public async Task AgregarAsync(Socio socio, CancellationToken ct = default)
        => await _context.Set<Socio>().AddAsync(socio, ct);

    public void Eliminar(Socio socio)
        => _context.Set<Socio>().Remove(socio);
}
