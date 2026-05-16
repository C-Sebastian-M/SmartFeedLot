using Feedlot.Domain.Common;
using Feedlot.Domain.Events;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Aggregate Root: ConsumoAlimenticio.
/// Representa el registro diario de alimento suministrado a un lote.
/// Se registra a nivel de lote porque el suministro es colectivo.
/// 
/// Invariante: la cantidad de kilogramos no puede ser negativa.
/// </summary>
public sealed class ConsumoAlimenticio : AggregateRoot<Guid>
{
    private ConsumoAlimenticio() { } // EF Core

    private ConsumoAlimenticio(
        Guid id,
        Guid loteId,
        Guid racionId,
        DateOnly fecha,
        CantidadKilogramos cantidadKg,
        Dinero costoTotal,
        Guid registradoPorId) : base(id)
    {
        LoteId = loteId;
        RacionId = racionId;
        Fecha = fecha;
        CantidadKg = cantidadKg;
        CostoTotal = costoTotal;
        RegistradoPorId = registradoPorId;
    }

    public Guid LoteId { get; private set; }
    public Guid RacionId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public CantidadKilogramos CantidadKg { get; private set; } = null!;
    public Dinero CostoTotal { get; private set; } = null!;
    public Guid RegistradoPorId { get; private set; }

    public static ConsumoAlimenticio Registrar(
        Guid loteId,
        Guid racionId,
        DateOnly fecha,
        CantidadKilogramos cantidadKg,
        Dinero costoTotal,
        Guid registradoPorId)
    {
        if (loteId == Guid.Empty)
            throw new DomainException("El ID del lote es requerido para registrar consumo.");

        if (racionId == Guid.Empty)
            throw new DomainException("El ID de la ración es requerido para registrar consumo.");

        var consumo = new ConsumoAlimenticio(
            Guid.NewGuid(), loteId, racionId, fecha, cantidadKg, costoTotal, registradoPorId);

        consumo.RaiseDomainEvent(new ConsumoAlimenticioRegistradoEvent(
            consumo.Id, loteId, racionId, cantidadKg.Valor, costoTotal.Monto, fecha));

        return consumo;
    }

    /// <summary>
    /// Corrige la cantidad registrada. Útil cuando hubo un error de entrada.
    /// Mantiene trazabilidad: no se elimina, se corrige con auditoría.
    /// </summary>
    public void CorregirCantidad(CantidadKilogramos nuevaCantidad, Dinero nuevoCosto)
    {
        CantidadKg = nuevaCantidad;
        CostoTotal = nuevoCosto;
    }
}
