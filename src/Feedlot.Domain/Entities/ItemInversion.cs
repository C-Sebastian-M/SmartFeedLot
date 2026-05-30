using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class ItemInversion : Entity<Guid>
{
    private ItemInversion() { }

    internal ItemInversion(Guid id, Guid etapaInversionId, string producto, Dinero costo,
        string? observacion, EstadoItemInversion estado, decimal porcentajeAvance)
        : base(id)
    {
        EtapaInversionId = etapaInversionId;
        Producto = producto;
        Costo = costo;
        Observacion = observacion;
        Estado = estado;
        PorcentajeAvance = porcentajeAvance;
    }

    public Guid EtapaInversionId { get; private set; }
    public string Producto { get; private set; } = null!;
    public Dinero Costo { get; private set; } = null!;
    public string? Observacion { get; private set; }
    public EstadoItemInversion Estado { get; private set; }
    public decimal PorcentajeAvance { get; private set; }

    public void Actualizar(string producto, Dinero costo, string? observacion,
        EstadoItemInversion estado, decimal porcentajeAvance)
    {
        Producto = producto;
        Costo = costo;
        Observacion = observacion;
        Estado = estado;
        PorcentajeAvance = Math.Clamp(porcentajeAvance, 0, 100);
    }

    public void Avanzar(decimal nuevoPorcentaje)
    {
        PorcentajeAvance = Math.Clamp(nuevoPorcentaje, 0, 100);
        if (PorcentajeAvance >= 100)
            Estado = EstadoItemInversion.OK;
    }
}
