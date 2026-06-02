using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Registro de precio de referencia por kg para una especie/tipo en una fecha y fuente dadas.
///
/// Decisión de diseño — sin Domain Events:
///   PrecioMercado es un aggregate de referencia (datos de mercado externos).
///   No existe ningún proceso de negocio que deba reaccionar a su creación o
///   modificación (no dispara notificaciones, no actualiza otros aggregates, no
///   genera movimientos financieros). Si en el futuro se requiere, por ejemplo,
///   alertar cuando el precio supere un umbral o actualizar proyecciones de
///   rentabilidad automáticamente, se deberá agregar un PrecioMercadoRegistradoEvent
///   o PrecioMercadoActualizadoEvent con su handler correspondiente.
/// </summary>
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

    public void Modificar(DateOnly fecha, string especie, string tipo, decimal precioPorKg, string fuente)
    {
        if (string.IsNullOrWhiteSpace(especie))
            throw new DomainException("La especie es requerida.");

        if (string.IsNullOrWhiteSpace(tipo))
            throw new DomainException("El tipo es requerido.");

        if (precioPorKg <= 0)
            throw new DomainException("El precio por kg debe ser mayor a cero.");

        if (string.IsNullOrWhiteSpace(fuente))
            throw new DomainException("La fuente es requerida.");

        Fecha = fecha;
        Especie = especie.Trim();
        Tipo = tipo.Trim();
        PrecioPorKg = precioPorKg;
        Fuente = fuente.Trim();
    }
}
