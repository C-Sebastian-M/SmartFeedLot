using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.ValueObjects;

/// <summary>
/// Value Object que representa la capacidad de un lote.
/// Encapsula máximo y actual como una unidad cohesiva con sus propias reglas.
/// </summary>
public sealed class Capacidad : ValueObject
{
    public int Maxima { get; }
    public int Actual { get; private set; }

    private Capacidad(int maxima, int actual)
    {
        Maxima = maxima;
        Actual = actual;
    }

    public static Capacidad Crear(int maxima, int actual = 0)
    {
        if (maxima <= 0)
            throw new DomainException(
                $"La capacidad máxima del lote debe ser mayor a cero. Recibido: {maxima}.");

        if (actual < 0)
            throw new DomainException(
                $"La cantidad actual de animales no puede ser negativa. Recibido: {actual}.");

        if (actual > maxima)
            throw new DomainException(
                $"La cantidad actual ({actual}) no puede superar la capacidad máxima ({maxima}).");

        return new Capacidad(maxima, actual);
    }

    public bool TieneEspacio => Actual < Maxima;
    public int Disponible => Maxima - Actual;
    public decimal PorcentajeOcupacion => Maxima == 0 ? 0 : (decimal)Actual / Maxima * 100;

    public Capacidad ConAnimalAgregado()
    {
        if (!TieneEspacio)
            throw new DomainException(
                $"La capacidad máxima ({Maxima}) ya fue alcanzada. No hay espacio disponible.");

        return new Capacidad(Maxima, Actual + 1);
    }

    public Capacidad ConAnimalRetirado()
    {
        if (Actual == 0)
            throw new DomainException("No hay animales en el lote para retirar.");

        return new Capacidad(Maxima, Actual - 1);
    }

    public override string ToString() => $"{Actual}/{Maxima} animales";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Maxima;
        yield return Actual;
    }
}
