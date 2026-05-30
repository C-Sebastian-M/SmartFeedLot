using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class CategoriaGastoRepository : ICategoriaGastoRepository
{
    private readonly FeedlotDbContext _context;

    public CategoriaGastoRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<CategoriaGasto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<CategoriaGasto>().FindAsync(new object[] { id }, ct);

    public async Task<CategoriaGasto?> ObtenerPorNombreAsync(string nombre, CancellationToken ct = default)
        => await _context.Set<CategoriaGasto>()
            .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower().Trim(), ct);

    public async Task<IReadOnlyList<CategoriaGasto>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<CategoriaGasto>().OrderBy(c => c.Nombre).ToListAsync(ct);

    public async Task AgregarAsync(CategoriaGasto categoria, CancellationToken ct = default)
        => await _context.Set<CategoriaGasto>().AddAsync(categoria, ct);

    public void Eliminar(CategoriaGasto categoria)
        => _context.Set<CategoriaGasto>().Remove(categoria);
}
