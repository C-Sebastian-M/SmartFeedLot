using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class ModuloSistemaRepository : IModuloSistemaRepository
{
    private readonly FeedlotDbContext _context;
    public ModuloSistemaRepository(FeedlotDbContext context) => _context = context;

    public async Task<IReadOnlyList<ModuloSistema>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<ModuloSistema>()
            .OrderBy(m => m.Orden)
            .ToListAsync(ct);

    public async Task<ModuloSistema?> ObtenerPorClaveAsync(string clave, CancellationToken ct = default)
        => await _context.Set<ModuloSistema>()
            .FirstOrDefaultAsync(m => m.Clave == clave.Trim().ToLower(), ct);
}
