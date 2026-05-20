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

    /// <summary>
    /// Consulta optimizada para la invariante de pertenencia única.
    /// Busca directamente en la tabla animal_lotes por AnimalId y EsActivo = true,
    /// luego carga el lote completo. Usa el índice ix_animal_lotes_animal_activo.
    /// </summary>
    public async Task<Lote?> ObtenerLoteActivoDelAnimalAsync(
        Guid animalId, CancellationToken ct = default)
    {
        var loteId = await _context.AnimalesLote
            .Where(al => al.AnimalId == animalId && al.EsActivo)
            .Select(al => al.LoteId)
            .FirstOrDefaultAsync(ct);

        if (loteId == Guid.Empty)
            return null;

        return await ObtenerPorIdAsync(loteId, ct);
    }

    public async Task AgregarAsync(Lote lote, CancellationToken ct = default)
        => await _context.Lotes.AddAsync(lote, ct);

    public void Actualizar(Lote lote)
        => _context.Lotes.Update(lote);
}
