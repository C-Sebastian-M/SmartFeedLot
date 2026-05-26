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

    public async Task<Animal?> ObtenerPorCodigoAsync(
        string codigo, CancellationToken ct = default)
    {
        var normalizado = codigo.Trim().ToUpperInvariant();

        // FromSqlRaw para filtrar por columna con HasConversion.
        // EF Core hidrata el aggregate completo correctamente.
        var id = await _context.Animals
            .FromSqlRaw(
                "SELECT * FROM feedlot.animals WHERE codigo_identificacion = {0}",
                normalizado)
            .Select(a => (Guid?)a.Id)
            .FirstOrDefaultAsync(ct);

        return id is null ? null : await ObtenerPorIdAsync(id.Value, ct);
    }

    public async Task<IReadOnlyList<Animal>> ObtenerTodosAsync(CancellationToken ct = default)
    {
        // Sin filtro SQL — se ordena en memoria por el VO.
        var animales = await _context.Animals
            .Include(a => a.Pesajes)
            .Include(a => a.EventosSanitarios)
            .ToListAsync(ct);

        return animales
            .OrderBy(a => a.CodigoIdentificacion.Valor)
            .ToList();
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var normalizado = codigo.Trim().ToUpperInvariant();

        // FromSqlRaw: consulta SQL directa, sin pasar por el ValueConverter.
        return await _context.Animals
            .FromSqlRaw(
                "SELECT * FROM feedlot.animals WHERE codigo_identificacion = {0}",
                normalizado)
            .AnyAsync(ct);
    }

    public async Task AgregarAsync(Animal animal, CancellationToken ct = default)
        => await _context.Animals.AddAsync(animal, ct);

    public void Actualizar(Animal animal)
        => _context.Animals.Update(animal);
}
