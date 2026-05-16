using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una cantidad de alimento en kilogramos.
/// Invariante: no puede ser negativa (regla de negocio explícita del feedlot).
/// </summary>
public sealed class CantidadKilogramos : ValueObject
{
    public decimal Valor { get; }

    private CantidadKilogramos(decimal valor)
    {
        Valor = valor;
    }

    public static CantidadKilogramos Crear(decimal valor)
    {
        if (valor < 0)
            throw new DomainException(
                $"La cantidad en kilogramos no puede ser negativa. Recibido: {valor} kg.");

        return new CantidadKilogramos(valor);
    }

    public static CantidadKilogramos Cero() => new(0);

    public CantidadKilogramos Sumar(CantidadKilogramos otra)
        => new(Valor + otra.Valor);

    public override string ToString() => $"{Valor:F2} kg";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }
}
