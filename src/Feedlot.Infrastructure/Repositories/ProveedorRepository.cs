using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class ProveedorRepository : IProveedorRepository
{
    private readonly FeedlotDbContext _context;

    public ProveedorRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Proveedor>().FindAsync([id], ct);

    public async Task<IReadOnlyList<Proveedor>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<Proveedor>().OrderBy(p => p.Nombre).ToListAsync(ct);

    public async Task AgregarAsync(Proveedor proveedor, CancellationToken ct = default)
        => await _context.Set<Proveedor>().AddAsync(proveedor, ct);

    public void Actualizar(Proveedor proveedor)
        => _context.Set<Proveedor>().Update(proveedor);

    public async Task<bool> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var proveedor = await _context.Set<Proveedor>().FindAsync([id], ct);
        if (proveedor is null) return false;
        _context.Set<Proveedor>().Remove(proveedor);
        return true;
    }
}