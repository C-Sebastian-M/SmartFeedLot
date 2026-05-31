using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Repositories;

public sealed class PresupuestoRepository : IPresupuestoRepository
{
    private readonly FeedlotDbContext _context;

    public PresupuestoRepository(FeedlotDbContext context)
    {
        _context = context;
    }

    public async Task<Presupuesto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<Presupuesto>()
            .Include(p => p.CategoriaGasto)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Presupuesto?> ObtenerPorPeriodoCategoriaAsync(
        int anio, int mes, Guid categoriaGastoId, CancellationToken ct = default)
        => await _context.Set<Presupuesto>()
            .Include(p => p.CategoriaGasto)
            .FirstOrDefaultAsync(p =>
                p.PeriodoAnio == anio &&
                p.PeriodoMes == mes &&
                p.CategoriaGastoId == categoriaGastoId, ct);

    public async Task<IReadOnlyList<Presupuesto>> ObtenerPorPeriodoAsync(
        int anio, int? mes = null, CancellationToken ct = default)
    {
        var query = _context.Set<Presupuesto>()
            .Include(p => p.CategoriaGasto)
            .Where(p => p.PeriodoAnio == anio);

        if (mes.HasValue)
            query = query.Where(p => p.PeriodoMes == mes.Value);

        return await query.OrderBy(p => p.PeriodoMes).ThenBy(p => p.CategoriaGasto.Nombre).ToListAsync(ct);
    }

    public async Task AgregarAsync(Presupuesto presupuesto, CancellationToken ct = default)
        => await _context.Set<Presupuesto>().AddAsync(presupuesto, ct);

    public void Actualizar(Presupuesto presupuesto)
        => _context.Set<Presupuesto>().Update(presupuesto);

    public void Eliminar(Presupuesto presupuesto)
        => _context.Set<Presupuesto>().Remove(presupuesto);
}
