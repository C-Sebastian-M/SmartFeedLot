using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.ValueObjects;

/// <summary>
/// Value Object para el código de identificación del animal (caravana/arete).
/// Valida formato: alfanumérico, entre 3 y 20 caracteres, sin espacios.
/// </summary>
public sealed class CodigoIdentificacion : ValueObject
{
    public string Valor { get; }

    private CodigoIdentificacion(string valor)
    {
        Valor = valor;
    }

    public static CodigoIdentificacion Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("El código de identificación no puede estar vacío.");

        valor = valor.Trim().ToUpperInvariant();

        if (valor.Length < 3 || valor.Length > 20)
            throw new DomainException(
                $"El código de identificación debe tener entre 3 y 20 caracteres. " +
                $"Recibido: '{valor}' ({valor.Length} caracteres).");

        if (!valor.All(c => char.IsLetterOrDigit(c) || c == '-'))
            throw new DomainException(
                $"El código de identificación solo puede contener letras, números y guiones. " +
                $"Valor inválido: '{valor}'.");

        return new CodigoIdentificacion(valor);
    }

    public override string ToString() => Valor;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }
}
