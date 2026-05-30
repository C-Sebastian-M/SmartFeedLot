using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class MovimientoFinanciero : AggregateRoot<Guid>
{
    private MovimientoFinanciero() { }

    private MovimientoFinanciero(
        Guid id,
        DateOnly fecha,
        int periodoAnio,
        int periodoMes,
        Guid categoriaGastoId,
        Dinero monto,
        OrigenFinanciero origen,
        string descripcion,
        Guid? socioId,
        Guid registradoPorId) : base(id)
    {
        Fecha = fecha;
        PeriodoAnio = periodoAnio;
        PeriodoMes = periodoMes;
        CategoriaGastoId = categoriaGastoId;
        Monto = monto;
        Origen = origen;
        Descripcion = descripcion;
        SocioId = socioId;
        RegistradoPorId = registradoPorId;
    }

    public DateOnly Fecha { get; private set; }
    public int PeriodoAnio { get; private set; }
    public int PeriodoMes { get; private set; }
    public Guid CategoriaGastoId { get; private set; }
    public CategoriaGasto CategoriaGasto { get; private set; } = null!;
    public Dinero Monto { get; private set; } = null!;
    public OrigenFinanciero Origen { get; private set; }
    public string Descripcion { get; private set; } = null!;
    public Guid? SocioId { get; private set; }
    public Socio? Socio { get; private set; }
    public Guid RegistradoPorId { get; private set; }

    public static MovimientoFinanciero Registrar(
        DateOnly fecha,
        int periodoAnio,
        int periodoMes,
        Guid categoriaGastoId,
        Dinero monto,
        OrigenFinanciero origen,
        string descripcion,
        Guid? socioId,
        Guid registradoPorId)
    {
        if (periodoAnio < 2000 || periodoAnio > 2100)
            throw new DomainException("El año del periodo no es válido.");

        if (periodoMes < 1 || periodoMes > 12)
            throw new DomainException("El mes del periodo debe estar entre 1 y 12.");

        if (categoriaGastoId == Guid.Empty)
            throw new DomainException("El ID de la categoría de gasto es requerido.");

        if (monto.Monto <= 0)
            throw new DomainException($"El monto del movimiento debe ser mayor a cero. Recibido: {monto.Monto}.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción del movimiento no puede estar vacía.");

        return new MovimientoFinanciero(
            Guid.NewGuid(),
            fecha,
            periodoAnio,
            periodoMes,
            categoriaGastoId,
            monto,
            origen,
            descripcion.Trim(),
            socioId,
            registradoPorId);
    }

    public void Modificar(
        DateOnly fecha,
        int periodoAnio,
        int periodoMes,
        Guid categoriaGastoId,
        Dinero monto,
        OrigenFinanciero origen,
        string descripcion,
        Guid? socioId)
    {
        if (periodoAnio < 2000 || periodoAnio > 2100)
            throw new DomainException("El año del periodo no es válido.");

        if (periodoMes < 1 || periodoMes > 12)
            throw new DomainException("El mes del periodo debe estar entre 1 y 12.");

        if (categoriaGastoId == Guid.Empty)
            throw new DomainException("El ID de la categoría de gasto es requerido.");

        if (monto.Monto <= 0)
            throw new DomainException($"El monto del movimiento debe ser mayor a cero. Recibido: {monto.Monto}.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción del movimiento no puede estar vacía.");

        Fecha = fecha;
        PeriodoAnio = periodoAnio;
        PeriodoMes = periodoMes;
        CategoriaGastoId = categoriaGastoId;
        Monto = monto;
        Origen = origen;
        Descripcion = descripcion.Trim();
        SocioId = socioId;
    }
}
