namespace Feedlot.Domain.Exceptions;

/// <summary>
/// Se lanza cuando un lote alcanza su capacidad máxima y se intenta
/// agregar otro animal.
/// </summary>
public sealed class LoteCapacidadExcedidaException : DomainException
{
    public LoteCapacidadExcedidaException(Guid loteId, int capacidadMaxima)
        : base($"El lote '{loteId}' ha alcanzado su capacidad máxima de {capacidadMaxima} animales.") { }
}
