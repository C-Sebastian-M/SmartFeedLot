using Feedlot.Domain.Common;

namespace Feedlot.Domain.Events;

/// <summary>
/// Se emite cuando se registra un nuevo pesaje sobre un animal.
/// El contexto de Analítica lo consume para recalcular GMD y detectar ineficiencias.
/// </summary>
public sealed record PesajeRegistradoEvent(
    Guid AnimalId,
    Guid PesajeId,
    decimal PesoKg,
    DateOnly FechaPesaje) : IDomainEvent;
