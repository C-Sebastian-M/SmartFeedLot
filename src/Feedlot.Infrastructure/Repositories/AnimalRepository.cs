using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class AnimalRepository : IAnimalRepository
{
    private readonly FeedlotDbContext _context;

    public AnimalRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Animal?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Animals
            .Include(a => a.Pesajes)
            .Include(a => a.EventosSanitarios)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Animal?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
        => await _context.Animals
            .Include(a => a.Pesajes)
            .Include(a => a.EventosSanitarios)
            .FirstOrDefaultAsync(a =>
                EF.Property<string>(a, "codigo_identificacion") == codigo.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Animal>> ObtenerTodosAsync(CancellationToken ct = default)
    {
        // CORRECCIÓN: no usar EF.Property en OrderBy con Value Objects convertidos.
        // Traer sin orden desde BD y ordenar en memoria — la lista de animales
        // en un feedlot típico es manejable (<5000). Para escala mayor se usaría
        // una query de proyección directa con Select().
        var animales = await _context.Animals
            .Include(a => a.Pesajes)
            .Include(a => a.EventosSanitarios)
            .ToListAsync(ct);

        return animales
            .OrderBy(a => a.CodigoIdentificacion.Valor)
            .ToList();
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default)
        => await _context.Animals
            .AnyAsync(a =>
                EF.Property<string>(a, "codigo_identificacion") == codigo.ToUpperInvariant(), ct);

    public async Task AgregarAsync(Animal animal, CancellationToken ct = default)
        => await _context.Animals.AddAsync(animal, ct);

    public void Actualizar(Animal animal)
        => _context.Animals.Update(animal);
}
