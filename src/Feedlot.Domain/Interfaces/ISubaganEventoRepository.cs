using Feedlot.Domain.Entities;

namespace Feedlot.Domain.Interfaces;

public interface ISubaganEventoRepository
{
    Task<bool> ExisteAsync(int subaganEventoId, CancellationToken ct = default);
    Task<IReadOnlyList<SubaganEvento>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<SubaganEvento?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SubaganLote>> ObtenerLotesPorEventoAsync(Guid eventoId, CancellationToken ct = default);
    Task AgregarAsync(SubaganEvento evento, CancellationToken ct = default);
    void Eliminar(SubaganEvento evento);

    /// <summary>
    /// Para un evento (por su Guid interno), devuelve el precio/kg promedio ponderado
    /// por cantidad de animales, agrupado por código de tipo comercial (MC, ML, HV...).
    /// Lo usa el cálculo de valor de venta proyectado del lote.
    /// </summary>
    Task<IReadOnlyDictionary<string, decimal>> ObtenerPreciosPorTipoAsync(
        Guid eventoId, CancellationToken ct = default);
}
