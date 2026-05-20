using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class ConsumoAlimenticioRepository : IConsumoAlimenticioRepository
{
    private readonly FeedlotDbContext _context;

    public ConsumoAlimenticioRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<ConsumoAlimenticio?> ObtenerPorIdAsync(
        Guid id, CancellationToken ct = default)
        => await _context.Consumos.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ConsumoAlimenticio>> ObtenerPorLoteAsync(
        Guid loteId,
        DateOnly? desde = null,
        DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var query = _context.Consumos.Where(c => c.LoteId == loteId);

        if (desde.HasValue)
            query = query.Where(c => c.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(c => c.Fecha <= hasta.Value);

        return await query.OrderBy(c => c.Fecha).ToListAsync(ct);
    }

    /// <summary>
    /// Agrega directamente en la BD con SUM() — evita traer todos los registros
    /// a memoria. Usa el índice ix_consumos_lote_fecha.
    /// </summary>
    public async Task<decimal> SumarKilogramosPorLoteAsync(
        Guid loteId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
        => await _context.Consumos
            .Where(c => c.LoteId == loteId && c.Fecha >= desde && c.Fecha <= hasta)
            .SumAsync(c => EF.Property<decimal>(c, "cantidad_kg"), ct);

    public async Task<decimal> SumarCostoPorLoteAsync(
        Guid loteId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
        => await _context.Consumos
            .Where(c => c.LoteId == loteId && c.Fecha >= desde && c.Fecha <= hasta)
            .SumAsync(c => EF.Property<decimal>(c, "costo_total"), ct);

    public async Task AgregarAsync(ConsumoAlimenticio consumo, CancellationToken ct = default)
        => await _context.Consumos.AddAsync(consumo, ct);

    public void Actualizar(ConsumoAlimenticio consumo)
        => _context.Consumos.Update(consumo);
}
