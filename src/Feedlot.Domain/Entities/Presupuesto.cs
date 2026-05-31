using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Presupuesto mensual por categoría de gasto.
/// Representa cuánto se planea gastar en una categoría para un período (año/mes).
/// Permite la comparación real vs presupuesto (RF-052).
/// </summary>
public sealed class Presupuesto : AggregateRoot<Guid>
{
    private Presupuesto() { }

    private Presupuesto(
        Guid id,
        int periodoAnio,
        int periodoMes,
        Guid categoriaGastoId,
        Dinero montoPresupuestado,
        string? descripcion) : base(id)
    {
        PeriodoAnio = periodoAnio;
        PeriodoMes = periodoMes;
        CategoriaGastoId = categoriaGastoId;
        MontoPresupuestado = montoPresupuestado;
        Descripcion = descripcion;
    }

    public int PeriodoAnio { get; private set; }
    public int PeriodoMes { get; private set; }
    public Guid CategoriaGastoId { get; private set; }
    public CategoriaGasto CategoriaGasto { get; private set; } = null!;
    public Dinero MontoPresupuestado { get; private set; } = null!;
    public string? Descripcion { get; private set; }

    public static Presupuesto Crear(
        int periodoAnio,
        int periodoMes,
        Guid categoriaGastoId,
        Dinero montoPresupuestado,
        string? descripcion)
    {
        if (periodoAnio < 2000 || periodoAnio > 2100)
            throw new DomainException("El año del período no es válido.");

        if (periodoMes < 1 || periodoMes > 12)
            throw new DomainException("El mes del período debe estar entre 1 y 12.");

        if (categoriaGastoId == Guid.Empty)
            throw new DomainException("La categoría de gasto es requerida.");

        if (montoPresupuestado.Monto < 0)
            throw new DomainException("El monto presupuestado no puede ser negativo.");

        return new Presupuesto(
            Guid.NewGuid(),
            periodoAnio,
            periodoMes,
            categoriaGastoId,
            montoPresupuestado,
            descripcion?.Trim());
    }

    public void Modificar(Dinero montoPresupuestado, string? descripcion)
    {
        if (montoPresupuestado.Monto < 0)
            throw new DomainException("El monto presupuestado no puede ser negativo.");

        MontoPresupuestado = montoPresupuestado;
        Descripcion = descripcion?.Trim();
    }
}
