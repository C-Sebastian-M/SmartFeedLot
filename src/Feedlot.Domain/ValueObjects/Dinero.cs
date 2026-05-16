using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.ValueObjects;

/// <summary>
/// Value Object que representa dinero. Encapsula monto y moneda.
/// Previene operar montos de distintas monedas accidentalmente.
/// </summary>
public sealed class Dinero : ValueObject
{
    public decimal Monto { get; }
    public string Moneda { get; }

    private Dinero(decimal monto, string moneda)
    {
        Monto = monto;
        Moneda = moneda;
    }

    public static Dinero Crear(decimal monto, string moneda = "COP")
    {
        if (monto < 0)
            throw new DomainException(
                $"El monto de dinero no puede ser negativo. Recibido: {monto}.");

        if (string.IsNullOrWhiteSpace(moneda) || moneda.Length != 3)
            throw new DomainException(
                $"La moneda debe ser un código ISO 4217 de 3 caracteres. Recibido: '{moneda}'.");

        return new Dinero(monto, moneda.ToUpperInvariant());
    }

    public static Dinero Cero(string moneda = "COP") => new(0, moneda);

    public Dinero Sumar(Dinero otro)
    {
        ValidarMismaMoneda(otro);
        return new Dinero(Monto + otro.Monto, Moneda);
    }

    public Dinero Restar(Dinero otro)
    {
        ValidarMismaMoneda(otro);
        return new Dinero(Monto - otro.Monto, Moneda);
    }

    public Dinero Multiplicar(decimal factor)
        => new(Monto * factor, Moneda);

    private void ValidarMismaMoneda(Dinero otro)
    {
        if (Moneda != otro.Moneda)
            throw new DomainException(
                $"No se pueden operar montos de distintas monedas: '{Moneda}' y '{otro.Moneda}'.");
    }

    public override string ToString() => $"{Monto:N2} {Moneda}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Monto;
        yield return Moneda;
    }
}
