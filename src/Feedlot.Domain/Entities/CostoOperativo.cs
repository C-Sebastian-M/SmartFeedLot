using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Aggregate Root: CostoOperativo.
/// 
/// Representa un costo del período que NO es alimento directo (ConsumoAlimenticio).
/// Cubre las dos categorías faltantes del modelo de costeo del Excel:
/// 
/// MANO DE OBRA: suministrar alimentación, preparar silo, fumigación de potreros,
///   mantenimiento de alambre y postes, lavado, riego/bombeo.
/// 
/// CIF (Costos Indirectos de Fabricación): gasolina moto bomba y picadora,
///   grama fin (matamaleza), cal agrícola, urea, alquiler de potrero.
/// 
/// Se registra a nivel de LOTE para el período y se prorratea entre animales
/// al calcular el costo unitario real, igual que lo hace el Excel.
/// </summary>
public sealed class CostoOperativo : AggregateRoot<Guid>
{
    private CostoOperativo() { } // EF Core

    private CostoOperativo(
        Guid id,
        Guid loteId,
        CategoriaCosto categoria,
        string concepto,
        DateOnly fecha,
        Dinero monto,
        string? observaciones,
        Guid registradoPorId) : base(id)
    {
        LoteId = loteId;
        Categoria = categoria;
        Concepto = concepto;
        Fecha = fecha;
        Monto = monto;
        Observaciones = observaciones;
        RegistradoPorId = registradoPorId;
    }

    public Guid LoteId { get; private set; }
    public CategoriaCosto Categoria { get; private set; }

    /// <summary>
    /// Descripción del costo. Ej: "Suministrar alimentación", "Gasolina moto bomba",
    /// "Alquiler potrero", "Fumigación de potreros y caña".
    /// </summary>
    public string Concepto { get; private set; } = null!;
    public DateOnly Fecha { get; private set; }
    public Dinero Monto { get; private set; } = null!;
    public string? Observaciones { get; private set; }
    public Guid RegistradoPorId { get; private set; }

    public static CostoOperativo Registrar(
        Guid loteId,
        CategoriaCosto categoria,
        string concepto,
        DateOnly fecha,
        Dinero monto,
        string? observaciones,
        Guid registradoPorId)
    {
        if (loteId == Guid.Empty)
            throw new DomainException("El ID del lote es requerido.");

        if (string.IsNullOrWhiteSpace(concepto))
            throw new DomainException("El concepto del costo no puede estar vacío.");

        if (monto.Monto <= 0)
            throw new DomainException(
                $"El monto del costo debe ser mayor a cero. Recibido: {monto.Monto}.");

        return new CostoOperativo(
            Guid.NewGuid(),
            loteId,
            categoria,
            concepto.Trim(),
            fecha,
            monto,
            observaciones?.Trim(),
            registradoPorId);
    }

    public void Corregir(Dinero nuevoMonto, string? nuevasObservaciones)
    {
        if (nuevoMonto.Monto <= 0)
            throw new DomainException("El monto corregido debe ser mayor a cero.");

        Monto = nuevoMonto;
        Observaciones = nuevasObservaciones?.Trim();
    }
}
