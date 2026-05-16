namespace Feedlot.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta agregar un animal a un lote que ya está en
/// otro lote activo simultáneamente.
/// Invariante crítica: un animal solo puede pertenecer a un lote activo a la vez.
/// </summary>
public sealed class AnimalYaEnLoteActivoException : DomainException
{
    public AnimalYaEnLoteActivoException(Guid animalId, Guid loteActivoId)
        : base($"El animal '{animalId}' ya pertenece al lote activo '{loteActivoId}'. " +
               "Un animal no puede estar en dos lotes activos simultáneamente.") { }
}
