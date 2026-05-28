using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class CompraRepository : ICompraRepository
{
    private readonly FeedlotDbContext _context;

    public CompraRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Compra?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Compra>().FindAsync([id], ct);

    public async Task<IReadOnlyList<Compra>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<Compra>().OrderByDescending(c => c.Fecha).ToListAsync(ct);

    public async Task<IReadOnlyList<Compra>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken ct = default)
        => await _context.Set<Compra>().Where(c => c.ProveedorId == proveedorId).OrderByDescending(c => c.Fecha).ToListAsync(ct);

    public async Task AgregarAsync(Compra compra, CancellationToken ct = default)
        => await _context.Set<Compra>().AddAsync(compra, ct);
}