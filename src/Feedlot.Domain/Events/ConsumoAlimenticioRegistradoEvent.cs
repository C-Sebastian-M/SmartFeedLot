using Feedlot.Domain.Common;

namespace Feedlot.Domain.Events;

/// <summary>
/// Se emite cuando se registra un consumo alimenticio en un lote.
/// El contexto de Analítica lo consume para recalcular ICA y costo por kg ganado.
/// </summary>
public sealed record ConsumoAlimenticioRegistradoEvent(
    Guid ConsumoId,
    Guid LoteId,
    Guid RacionId,
    decimal CantidadKg,
    decimal CostoTotal,
    DateOnly Fecha) : IDomainEvent;
