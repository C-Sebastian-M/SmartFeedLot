using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un peso en kilogramos.
/// Garantiza que el valor sea positivo. Inmutable.
/// Se usa en pesajes, peso de ingreso y comparaciones productivas.
/// </summary>
public sealed class Peso : ValueObject
{
    public decimal Kilogramos { get; }

    private Peso(decimal kilogramos)
    {
        Kilogramos = kilogramos;
    }

    /// <summary>
    /// Factory method con validación de invariantes.
    /// Lanza DomainException si el valor es negativo o cero.
    /// </summary>
    public static Peso Crear(decimal kilogramos)
    {
        if (kilogramos <= 0)
            throw new DomainException(
                $"El peso debe ser mayor a cero. Valor recibido: {kilogramos} kg.");

        return new Peso(kilogramos);
    }

    /// <summary>Calcula la diferencia en kilogramos entre dos pesos.</summary>
    public decimal DiferenciaKg(Peso pesoInicial)
        => Kilogramos - pesoInicial.Kilogramos;

    public override string ToString() => $"{Kilogramos:F2} kg";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kilogramos;
    }
}
