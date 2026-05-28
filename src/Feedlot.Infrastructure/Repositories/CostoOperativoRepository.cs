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

    public async Task<CostoOperativo?> ObtenerPorIdAsync(
        Guid id, CancellationToken ct = default)
        => await _context.CostosOperativos
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CostoOperativo>> ObtenerPorLoteAsync(
        Guid loteId,
        DateOnly? desde = null,
        DateOnly? hasta = null,
        CategoriaCosto? categoria = null,
        CancellationToken ct = default)
    {
        var query = _context.CostosOperativos
            .Where(c => c.LoteId == loteId);

        if (desde.HasValue)
            query = query.Where(c => c.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(c => c.Fecha <= hasta.Value);

        if (categoria.HasValue)
            query = query.Where(c => c.Categoria == categoria.Value);

        // Traer a memoria y ordenar — evita problemas de traducción
        // de HasConversion con OrderBy en EF Core + Npgsql.
        var resultados = await query.ToListAsync(ct);
        return resultados.OrderBy(c => c.Fecha).ToList();
    }

    public async Task<decimal> SumarMontoPorLoteAsync(
        Guid loteId,
        DateOnly desde,
        DateOnly hasta,
        CategoriaCosto? categoria = null,
        CancellationToken ct = default)
    {
        if (categoria.HasValue)
        {
            return await _context.Database
                .SqlQueryRaw<decimal>(
                    """SELECT COALESCE(SUM(monto), 0) AS "Value" FROM feedlot.costos_operativos WHERE lote_id = {0} AND fecha >= {1} AND fecha <= {2} AND categoria = {3}""",
                    loteId, desde, hasta, categoria.Value.ToString())
                .SingleAsync(ct);
        }

        return await _context.Database
            .SqlQueryRaw<decimal>(
                """SELECT COALESCE(SUM(monto), 0) AS "Value" FROM feedlot.costos_operativos WHERE lote_id = {0} AND fecha >= {1} AND fecha <= {2}""",
                loteId, desde, hasta)
            .SingleAsync(ct);
    }

    public async Task AgregarAsync(
        CostoOperativo costo, CancellationToken ct = default)
        => await _context.CostosOperativos.AddAsync(costo, ct);

    public void Actualizar(CostoOperativo costo)
        => _context.CostosOperativos.Update(costo);
}
