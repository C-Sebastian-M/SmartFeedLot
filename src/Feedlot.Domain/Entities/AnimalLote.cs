using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Entity interna del aggregate Lote.
/// Representa la relación histórica entre un animal y un lote.
/// Es la trazabilidad de movimientos — cada ingreso/egreso queda registrado.
/// FechaEgreso nulo significa que el animal aún está activo en este lote.
/// </summary>
public sealed class AnimalLote : Entity<Guid>
{
    private AnimalLote() { } // EF Core

    private AnimalLote(
        Guid id,
        Guid loteId,
        Guid animalId,
        DateOnly fechaIngreso,
        MotivoMovimiento motivoIngreso) : base(id)
    {
        LoteId = loteId;
        AnimalId = animalId;
        FechaIngreso = fechaIngreso;
        MotivoIngreso = motivoIngreso;
        EsActivo = true;
    }

    public Guid LoteId { get; private set; }
    public Guid AnimalId { get; private set; }
    public DateOnly FechaIngreso { get; private set; }
    public DateOnly? FechaEgreso { get; private set; }
    public MotivoMovimiento MotivoIngreso { get; private set; }
    public MotivoMovimiento? MotivoEgreso { get; private set; }
    public bool EsActivo { get; private set; }

    /// <summary>Días que el animal estuvo (o lleva) en este lote.</summary>
    public int DiasEnLote
    {
        get
        {
            var fechaFin = FechaEgreso ?? DateOnly.FromDateTime(DateTime.UtcNow);
            return fechaFin.DayNumber - FechaIngreso.DayNumber;
        }
    }

    internal static AnimalLote Crear(
        Guid loteId,
        Guid animalId,
        DateOnly fechaIngreso,
        MotivoMovimiento motivoIngreso)
    {
        return new AnimalLote(Guid.NewGuid(), loteId, animalId, fechaIngreso, motivoIngreso);
    }

    internal void ModificarFechaIngreso(DateOnly nuevaFechaIngreso)
    {
        if (EsActivo && nuevaFechaIngreso > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("La fecha de ingreso no puede ser futura.");

        FechaIngreso = nuevaFechaIngreso;
    }

    internal void Cerrar(DateOnly fechaEgreso, MotivoMovimiento motivoEgreso)
    {
        if (!EsActivo)
            throw new DomainException(
                $"El registro de animal en lote '{Id}' ya está cerrado.");

        if (fechaEgreso < FechaIngreso)
            throw new DomainException(
                $"La fecha de egreso ({fechaEgreso}) no puede ser anterior a la de ingreso ({FechaIngreso}).");

        FechaEgreso = fechaEgreso;
        MotivoEgreso = motivoEgreso;
        EsActivo = false;
    }
}
