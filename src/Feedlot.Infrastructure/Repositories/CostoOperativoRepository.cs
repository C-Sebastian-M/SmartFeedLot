using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class CostoOperativoRepository : ICostoOperativoRepository
{
    private readonly FeedlotDbContext _context;
    public CostoOperativoRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public Task<CostoOperativo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => _context.CostosOperativos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CostoOperativo>> ObtenerPorLoteAsync(
        Guid loteId,
        DateOnly? desde = null,
        DateOnly? hasta = null,
        CategoriaCosto? categoria = null,
        CancellationToken ct = default)
    {
        var query = _context.CostosOperativos.Where(c => c.LoteId == loteId);

        if (desde.HasValue)
            query = query.Where(c => c.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(c => c.Fecha <= hasta.Value);

        if (categoria.HasValue)
            query = query.Where(c => c.Categoria == categoria.Value);

        return await query.AsNoTracking().ToListAsync(ct);
    }

    public async Task<decimal> SumarMontoPorLoteAsync(
        Guid loteId,
        DateOnly desde,
        DateOnly hasta,
        CategoriaCosto? categoria = null,
        CancellationToken ct = default)
    {
        var query = _context.CostosOperativos
            .Where(c => c.LoteId == loteId && c.Fecha >= desde && c.Fecha <= hasta);

        if (categoria.HasValue)
            query = query.Where(c => c.Categoria == categoria.Value);

        return await query.SumAsync(c => c.Monto.Monto, ct);
    }

    public async Task AgregarAsync(CostoOperativo costo, CancellationToken ct = default)
        => await _context.CostosOperativos.AddAsync(costo, ct);

    public void Actualizar(CostoOperativo costo)
        => _context.CostosOperativos.Update(costo);
}
