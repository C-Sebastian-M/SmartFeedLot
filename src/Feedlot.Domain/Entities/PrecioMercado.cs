using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

public sealed class PrecioMercado : AggregateRoot<Guid>
{
    private PrecioMercado() { }

    private PrecioMercado(Guid id, DateOnly fecha, string especie, string tipo,
        decimal precioPorKg, string fuente)
        : base(id)
    {
        Fecha = fecha;
        Especie = especie;
        Tipo = tipo;
        PrecioPorKg = precioPorKg;
        Fuente = fuente;
    }

    public DateOnly Fecha { get; private set; }
    public string Especie { get; private set; } = null!;
    public string Tipo { get; private set; } = null!;
    public decimal PrecioPorKg { get; private set; }
    public string Fuente { get; private set; } = null!;

    public static PrecioMercado Crear(DateOnly fecha, string especie, string tipo,
        decimal precioPorKg, string fuente)
    {
        if (string.IsNullOrWhiteSpace(especie))
            throw new DomainException("La especie es requerida.");

        if (string.IsNullOrWhiteSpace(tipo))
            throw new DomainException("El tipo es requerido.");

        if (precioPorKg <= 0)
            throw new DomainException("El precio por kg debe ser mayor a cero.");

        if (string.IsNullOrWhiteSpace(fuente))
            throw new DomainException("La fuente es requerida.");

        return new PrecioMercado(Guid.NewGuid(), fecha, especie.Trim(), tipo.Trim(),
            precioPorKg, fuente.Trim());
    }
}
