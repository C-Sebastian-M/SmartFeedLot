namespace Feedlot.Domain.Exceptions;

/// <summary>
/// Excepción base del dominio. Representa una violación de una regla de negocio.
/// No debe ser atrapada en el dominio — se propaga a la capa de Application
/// donde un behavior de MediatR la convierte en una respuesta apropiada.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
