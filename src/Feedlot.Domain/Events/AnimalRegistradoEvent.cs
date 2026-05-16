using Feedlot.Domain.Common;

namespace Feedlot.Domain.Events;

/// <summary>
/// Se emite cuando un nuevo animal es registrado en el sistema.
/// El contexto de Analítica lo consume para inicializar el seguimiento productivo.
/// </summary>
public sealed record AnimalRegistradoEvent(
    Guid AnimalId,
    string CodigoIdentificacion,
    decimal PesoIngresoKg,
    DateOnly FechaIngreso) : IDomainEvent;
