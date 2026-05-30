using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
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
        var animales = await _context.Animals
            .Include(a => a.Pesajes)
            .Include(a => a.EventosSanitarios)
            .ToListAsync(ct);

        return animales
            .OrderBy(a => a.CodigoIdentificacion.Valor)
            .ToList();
    }

    public async Task<IReadOnlyList<Animal>> ObtenerPorIdsAsync(
        IReadOnlyList<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return [];

        // Una sola consulta con Include de Pesajes para los N animales.
        // Reemplaza N llamadas individuales a ObtenerPorIdAsync (resuelve N+1).
        var animales = await _context.Animals
            .Include(a => a.Pesajes)
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(ct);

        return animales;
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var normalizado = codigo.Trim().ToUpperInvariant();

        return await _context.Animals
            .FromSqlRaw(
                "SELECT id FROM feedlot.animals WHERE codigo_identificacion = {0}",
                normalizado)
            .AnyAsync(ct);
    }

    public async Task<(IReadOnlyList<Animal> Items, int TotalCount)> ObtenerPaginadosAsync(
        int page,
        int pageSize,
        EstadoProductivo? estadoProductivo,
        EstadoSanitario? estadoSanitario,
        string? raza,
        string? busqueda,
        CancellationToken ct = default)
    {
        var query = _context.Animals.AsQueryable();

        if (estadoProductivo.HasValue)
            query = query.Where(a => a.EstadoProductivo == estadoProductivo.Value);

        if (estadoSanitario.HasValue)
            query = query.Where(a => a.EstadoSanitario == estadoSanitario.Value);

        if (!string.IsNullOrWhiteSpace(raza))
            query = query.Where(a => a.Raza.Contains(raza));

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var normalizado = busqueda.Trim().ToUpperInvariant();

            var idsPorCodigo = await _context.Animals
                .FromSqlRaw(
                    "SELECT * FROM feedlot.animals WHERE UPPER(codigo_identificacion) LIKE {0}",
                    $"%{normalizado}%")
                .Select(a => a.Id)
                .ToListAsync(ct);

            query = query.Where(a =>
                idsPorCodigo.Contains(a.Id) ||
                (a.Nombre != null && a.Nombre.Contains(normalizado)) ||
                a.NumeroArete.Contains(normalizado));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        items = items
            .OrderBy(a => a.CodigoIdentificacion.Valor)
            .ToList();

        return (items, totalCount);
    }

    public async Task<string> ObtenerSiguienteCodigoAsync(CancellationToken ct = default)
    {
        var maxCodigo = await _context.Database
            .SqlQueryRaw<string?>(
                """SELECT codigo_identificacion AS "Value" FROM feedlot.animals ORDER BY codigo_identificacion DESC LIMIT 1""")
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(maxCodigo))
            return "ANI-001";

        var partes = maxCodigo.Split('-');
        if (partes.Length != 2 || !int.TryParse(partes[1], out var numero))
            return "ANI-001";

        return $"ANI-{numero + 1:D3}";
    }

    public async Task<string> ObtenerSiguienteAreteAsync(CancellationToken ct = default)
    {
        var maxArete = await _context.Database
            .SqlQueryRaw<string?>(
                """SELECT numero_arete AS "Value" FROM feedlot.animals ORDER BY LENGTH(numero_arete) DESC, numero_arete DESC LIMIT 1""")
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(maxArete))
            return "AR-0001";

        var partes = maxArete.Split('-');
        if (partes.Length != 2 || !int.TryParse(partes[1], out var numero))
            return "AR-0001";

        return $"AR-{numero + 1:D4}";
    }

    public async Task<Dictionary<Guid, (string Codigo, string? Nombre)>> ObtenerCodigosPorIdsAsync(
        IReadOnlyList<Guid> ids, CancellationToken ct = default)
    {
        var dict = new Dictionary<Guid, (string, string?)>();
        if (ids.Count == 0) return dict;

        var animales = await _context.Animals
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(ct);

        foreach (var animal in animales)
            dict[animal.Id] = (animal.CodigoIdentificacion.Valor, animal.Nombre);

        return dict;
    }

    public async Task AgregarAsync(Animal animal, CancellationToken ct = default)
        => await _context.Animals.AddAsync(animal, ct);

    public void Actualizar(Animal animal)
        => _context.Animals.Update(animal);

    public async Task EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var animal = await _context.Animals
            .Include(a => a.Pesajes)
            .Include(a => a.EventosSanitarios)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (animal is null) return;

        var animalLotes = await _context.AnimalesLote
            .Where(al => al.AnimalId == id)
            .ToListAsync(ct);

        _context.RemoveRange(animal.Pesajes);
        _context.RemoveRange(animal.EventosSanitarios);
        _context.RemoveRange(animalLotes);
        _context.Remove(animal);
    }

    public async Task<IReadOnlyList<(Guid AnimalId, string Codigo, string? Nombre, string Diagnostico, DateOnly ProximaDosis, string? Responsable)>>
        ObtenerVacunasProximasAsync(int dias, CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = hoy.AddDays(dias);

        var resultado = await _context.EventosSanitarios
            .Where(e => e.TipoEvento == "Vacuna"
                && e.ProximaDosis != null
                && e.ProximaDosis >= hoy
                && e.ProximaDosis <= limite)
            .Join(_context.Animals, e => e.AnimalId, a => a.Id, (e, a) => new
            {
                a.Id,
                a.CodigoIdentificacion.Valor,
                a.Nombre,
                e.Diagnostico,
                e.ProximaDosis,
                e.Responsable
            })
            .ToListAsync(ct);

        return resultado
            .Select(r => (
                r.Id,
                r.Valor,
                r.Nombre,
                r.Diagnostico,
                r.ProximaDosis!.Value,
                r.Responsable))
            .ToList()
            .AsReadOnly();
    }
}
