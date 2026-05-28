using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class CompradorRepository : ICompradorRepository
{
    private readonly FeedlotDbContext _context;

    public CompradorRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Comprador?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Comprador>().FindAsync([id], ct);

    public async Task<IReadOnlyList<Comprador>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Set<Comprador>().OrderBy(c => c.Nombre).ToListAsync(ct);

    public async Task AgregarAsync(Comprador comprador, CancellationToken ct = default)
        => await _context.Set<Comprador>().AddAsync(comprador, ct);

    public void Actualizar(Comprador comprador)
        => _context.Set<Comprador>().Update(comprador);

    public async Task<bool> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var comprador = await _context.Set<Comprador>().FindAsync([id], ct);
        if (comprador is null) return false;
        _context.Set<Comprador>().Remove(comprador);
        return true;
    }

    public async Task<bool> ExisteConNombreAsync(string nombre, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Set<Comprador>().Where(c => c.Nombre == nombre.Trim());
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }
}
