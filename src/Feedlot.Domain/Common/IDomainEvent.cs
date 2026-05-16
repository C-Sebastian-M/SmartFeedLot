namespace Feedlot.Domain.Common;

/// <summary>
/// Marker interface para Domain Events.
/// Los domain events representan algo que ocurrió en el dominio y que otros
/// bounded contexts o handlers pueden reaccionar ante ello (via MediatR INotification).
/// </summary>
public interface IDomainEvent;
