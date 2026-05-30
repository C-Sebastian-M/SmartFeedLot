using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class MovimientoFinancieroRepository : IMovimientoFinancieroRepository
{
    private readonly FeedlotDbContext _context;

    public MovimientoFinancieroRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<MovimientoFinanciero?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<MovimientoFinanciero>()
            .Include(m => m.CategoriaGasto)
            .Include(m => m.Socio)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<MovimientoFinanciero>> ObtenerPorFiltroAsync(
        int? anio = null,
        int? mes = null,
        OrigenFinanciero? origen = null,
        Guid? categoriaGastoId = null,
        Guid? socioId = null,
        CancellationToken ct = default)
    {
        var query = _context.Set<MovimientoFinanciero>()
            .Include(m => m.CategoriaGasto)
            .Include(m => m.Socio)
            .AsQueryable();

        if (anio.HasValue)
            query = query.Where(m => m.PeriodoAnio == anio.Value);

        if (mes.HasValue)
            query = query.Where(m => m.PeriodoMes == mes.Value);

        if (origen.HasValue)
            query = query.Where(m => m.Origen == origen.Value);

        if (categoriaGastoId.HasValue)
            query = query.Where(m => m.CategoriaGastoId == categoriaGastoId.Value);

        if (socioId.HasValue)
            query = query.Where(m => m.SocioId == socioId.Value);

        return await query.OrderByDescending(m => m.Fecha).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MovimientoFinanciero>> ObtenerPorRangoFechasAsync(
        DateOnly desde,
        DateOnly hasta,
        OrigenFinanciero? origen = null,
        CancellationToken ct = default)
    {
        var query = _context.Set<MovimientoFinanciero>()
            .Include(m => m.CategoriaGasto)
            .Include(m => m.Socio)
            .Where(m => m.Fecha >= desde && m.Fecha <= hasta);

        if (origen.HasValue)
            query = query.Where(m => m.Origen == origen.Value);

        return await query.OrderByDescending(m => m.Fecha).ToListAsync(ct);
    }

    public async Task AgregarAsync(MovimientoFinanciero movimiento, CancellationToken ct = default)
        => await _context.Set<MovimientoFinanciero>().AddAsync(movimiento, ct);

    public void Actualizar(MovimientoFinanciero movimiento)
        => _context.Set<MovimientoFinanciero>().Update(movimiento);

    public void Eliminar(MovimientoFinanciero movimiento)
        => _context.Set<MovimientoFinanciero>().Remove(movimiento);
}
