using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Costo operativo de un lote: Mano de Obra o CIF (Costo Indirecto de Fabricación).
/// Se prorratea entre los animales del lote al calcular el costo unitario real.
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
                $"El monto debe ser mayor a cero. Recibido: {monto.Monto}.");

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
}
