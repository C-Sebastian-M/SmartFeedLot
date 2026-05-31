using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface IPresupuestoRepository
{
    Task<Presupuesto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<Presupuesto?> ObtenerPorPeriodoCategoriaAsync(
        int anio, int mes, Guid categoriaGastoId, CancellationToken ct = default);

    Task<IReadOnlyList<Presupuesto>> ObtenerPorPeriodoAsync(
        int anio, int? mes = null, CancellationToken ct = default);

    Task AgregarAsync(Presupuesto presupuesto, CancellationToken ct = default);
    void Actualizar(Presupuesto presupuesto);
    void Eliminar(Presupuesto presupuesto);
}
