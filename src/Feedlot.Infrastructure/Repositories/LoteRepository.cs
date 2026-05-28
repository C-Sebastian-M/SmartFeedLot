using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class LoteRepository : ILoteRepository
{
    private readonly FeedlotDbContext _context;

    public LoteRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Lote?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Lotes
            .Include(l => l.AnimalesLote)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<Lote?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default)
        => await _context.Lotes
            .Include(l => l.AnimalesLote)
            .FirstOrDefaultAsync(l => l.Codigo == codigo.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Lote>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _context.Lotes
            .Include(l => l.AnimalesLote)
            .OrderBy(l => l.Codigo)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Lote>> ObtenerActivosAsync(CancellationToken ct = default)
        => await _context.Lotes
            .Include(l => l.AnimalesLote)
            .Where(l => l.Estado == EstadoLote.Activo)
            .OrderBy(l => l.Codigo)
            .ToListAsync(ct);

    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default)
        => await _context.Lotes
            .AnyAsync(l => l.Codigo == codigo.ToUpperInvariant(), ct);

    public async Task<string> ObtenerSiguienteCodigoAsync(CancellationToken ct = default)
    {
        var maxCodigo = await _context.Lotes
            .Select(l => l.Codigo)
            .OrderByDescending(c => c.Length).ThenByDescending(c => c)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(maxCodigo))
            return "LOT-001";

        var partes = maxCodigo.Split('-');
        if (partes.Length != 2 || !int.TryParse(partes[1], out var numero))
            return "LOT-001";

        return $"LOT-{numero + 1:D3}";
    }

    public async Task<Lote?> ObtenerLoteActivoDelAnimalAsync(
        Guid animalId, CancellationToken ct = default)
    {
        var loteId = await _context.AnimalesLote
            .Where(al => al.AnimalId == animalId && al.EsActivo)
            .Select(al => (Guid?)al.LoteId)
            .FirstOrDefaultAsync(ct);

        if (loteId is null)
            return null;

        return await ObtenerPorIdAsync(loteId.Value, ct);
    }

    public async Task AgregarAsync(Lote lote, CancellationToken ct = default)
        => await _context.Lotes.AddAsync(lote, ct);

    public void Actualizar(Lote lote)
        => _context.Lotes.Update(lote);

    public async Task ActualizarFechaIngresoAnimalAsync(
        Guid animalId, DateOnly nuevaFechaIngreso, CancellationToken ct = default)
    {
        var animalLote = await _context.AnimalesLote
            .FirstOrDefaultAsync(al => al.AnimalId == animalId && al.EsActivo, ct);

        if (animalLote is not null)
        {
            var entry = _context.Entry(animalLote);
            entry.Property(al => al.FechaIngreso).CurrentValue = nuevaFechaIngreso;
            entry.Property(al => al.FechaIngreso).IsModified = true;
        }
    }
}
