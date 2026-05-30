using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class LoteSilo : AggregateRoot<Guid>
{
    private LoteSilo() { }

    private LoteSilo(Guid id, Guid? corteCaniaId, DateOnly fechaProduccion,
        int bolsas, Dinero costoUnitario, string observacion)
        : base(id)
    {
        CorteCaniaId = corteCaniaId;
        FechaProduccion = fechaProduccion;
        Bolsas = bolsas;
        CostoUnitario = costoUnitario;
        Observacion = observacion;
    }

    public Guid? CorteCaniaId { get; private set; }
    public DateOnly FechaProduccion { get; private set; }
    public int Bolsas { get; private set; }
    public Dinero CostoUnitario { get; private set; } = null!;
    public string? Observacion { get; private set; }

    public decimal CostoTotal => Math.Round(CostoUnitario.Monto * Bolsas, 2);

    public static LoteSilo Crear(DateOnly fechaProduccion, int bolsas,
        Dinero costoUnitario, string? observacion, Guid? corteCaniaId = null)
    {
        if (bolsas <= 0)
            throw new DomainException("El número de bolsas debe ser mayor a cero.");
        if (costoUnitario.Monto < 0)
            throw new DomainException("El costo unitario no puede ser negativo.");

        return new LoteSilo(Guid.NewGuid(), corteCaniaId, fechaProduccion,
            bolsas, costoUnitario, observacion?.Trim());
    }
}
