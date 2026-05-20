using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

/// <summary>
/// Implementación concreta de IAnimalRepository con EF Core.
/// Usa eager loading con Include() para cargar las colecciones internas
/// del aggregate (Pesajes, EventosSanitarios) en una sola query.
///
/// Decisión: siempre cargar el aggregate completo — el dominio necesita
/// todos sus datos para aplicar invariantes. En feedlots típicos los animales
/// no tienen miles de pesajes, así que el overhead es aceptable.
/// Para casos extremos, se puede proyectar con Select() en queries de solo lectura.
/// </summary>
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
        => await _context.Animals
            .Include(a => a.Pesajes)
            .OrderBy(a => EF.Property<string>(a, "codigo_identificacion"))
            .ToListAsync(ct);

    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default)
        => await _context.Animals
            .AnyAsync(a =>
                EF.Property<string>(a, "codigo_identificacion") == codigo.ToUpperInvariant(), ct);

    public async Task AgregarAsync(Animal animal, CancellationToken ct = default)
        => await _context.Animals.AddAsync(animal, ct);

    public void Actualizar(Animal animal)
        => _context.Animals.Update(animal);
}
