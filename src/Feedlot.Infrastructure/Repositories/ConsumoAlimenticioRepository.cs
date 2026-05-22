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
    /// CORRECCIÓN: en lugar de SumAsync sobre el Value Object (que EF Core no puede
    /// traducir a SQL directamente), usamos EF.Property para acceder a la columna
    /// subyacente por nombre, que SÍ se puede traducir a SUM() en PostgreSQL.
    /// </summary>
    public async Task<decimal> SumarKilogramosPorLoteAsync(
        Guid loteId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var consumos = await _context.Consumos
            .Where(c => c.LoteId == loteId && c.Fecha >= desde && c.Fecha <= hasta)
            .ToListAsync(ct);

        // Sumar en memoria usando el Value Object — correcto y type-safe.
        return consumos.Sum(c => c.CantidadKg.Valor);
    }

    public async Task<decimal> SumarCostoPorLoteAsync(
        Guid loteId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var consumos = await _context.Consumos
            .Where(c => c.LoteId == loteId && c.Fecha >= desde && c.Fecha <= hasta)
            .ToListAsync(ct);

        return consumos.Sum(c => c.CostoTotal.Monto);
    }

    public async Task AgregarAsync(ConsumoAlimenticio consumo, CancellationToken ct = default)
        => await _context.Consumos.AddAsync(consumo, ct);

    public void Actualizar(ConsumoAlimenticio consumo)
        => _context.Consumos.Update(consumo);
}
