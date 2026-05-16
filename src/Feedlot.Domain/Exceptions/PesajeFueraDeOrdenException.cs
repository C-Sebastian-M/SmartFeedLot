namespace Feedlot.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta registrar un pesaje con fecha anterior
/// al último pesaje registrado.
/// Invariante: los pesajes deben mantener orden cronológico estricto.
/// </summary>
public sealed class PesajeFueraDeOrdenException : DomainException
{
    public PesajeFueraDeOrdenException(DateOnly fechaNueva, DateOnly fechaUltima)
        : base($"El pesaje con fecha '{fechaNueva}' no puede ser anterior o igual " +
               $"al último pesaje registrado '{fechaUltima}'. " +
               "Los pesajes deben mantener orden cronológico.") { }
}
