using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;

namespace Feedlot.Domain.Events;

/// <summary>
/// Se emite cuando un animal es movido de un lote a otro (o ingresa por primera vez).
/// LoteOrigenId null = ingreso inicial al sistema.
/// LoteDestinoId null = egreso del sistema (venta, muerte).
/// </summary>
public sealed record AnimalMovidoALoteEvent(
    Guid AnimalId,
    Guid? LoteOrigenId,
    Guid? LoteDestinoId,
    DateOnly FechaMovimiento,
    MotivoMovimiento Motivo) : IDomainEvent;
