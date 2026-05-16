namespace Feedlot.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta una operación sobre un animal que no está activo.
/// Invariante: los animales inactivos (Vendido, Muerto, Retirado) no pueden
/// registrar nuevos eventos, pesajes ni movimientos.
/// </summary>
public sealed class AnimalInactivoException : DomainException
{
    public AnimalInactivoException(Guid animalId)
        : base($"El animal con ID '{animalId}' no está activo en engorde. " +
               "No se pueden registrar eventos sobre animales inactivos.") { }
}
