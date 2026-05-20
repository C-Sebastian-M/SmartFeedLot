using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class RacionRepository : IRacionRepository
{
    private readonly FeedlotDbContext _context;

    public RacionRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Racion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Raciones
            .Include(r => r.Ingredientes)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Racion>> ObtenerActivasAsync(CancellationToken ct = default)
        => await _context.Raciones
            .Include(r => r.Ingredientes)
            .Where(r => r.Activa)
            .OrderBy(r => r.Nombre)
            .ToListAsync(ct);

    public async Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default)
        => await _context.Raciones
            .AnyAsync(r => r.Nombre.ToLower() == nombre.ToLower(), ct);

    public async Task AgregarAsync(Racion racion, CancellationToken ct = default)
        => await _context.Raciones.AddAsync(racion, ct);

    public void Actualizar(Racion racion)
        => _context.Raciones.Update(racion);
}

public sealed class IngredienteRepository : IIngredienteRepository
{
    private readonly FeedlotDbContext _context;

    public IngredienteRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Ingrediente?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Ingredientes.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<Ingrediente>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Ingredientes.OrderBy(i => i.Nombre).ToListAsync(ct);

    public async Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default)
        => await _context.Ingredientes
            .AnyAsync(i => i.Nombre.ToLower() == nombre.ToLower(), ct);

    public async Task AgregarAsync(Ingrediente ingrediente, CancellationToken ct = default)
        => await _context.Ingredientes.AddAsync(ingrediente, ct);

    public void Actualizar(Ingrediente ingrediente)
        => _context.Ingredientes.Update(ingrediente);
}
