using Feedlot.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Feedlot.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Wrapper que convierte un IDomainEvent del dominio en un INotification de MediatR.
/// 
/// Decisión de diseño: el Domain no referencia MediatR directamente (dependency rule).
/// Infrastructure actúa como adaptador: envuelve el IDomainEvent en este wrapper
/// para que MediatR pueda despacharlo sin que el dominio conozca MediatR.
/// </summary>
internal sealed class DomainEventNotification<TEvent> : INotification
    where TEvent : IDomainEvent
{
    public TEvent DomainEvent { get; }

    public DomainEventNotification(TEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}

/// <summary>
/// Interceptor de EF Core que despacha Domain Events acumulados en los Aggregates
/// ANTES de hacer commit. 
/// 
/// Solo procesa entidades que heredan de Entity&lt;Guid&gt; (aggregates del dominio).
/// Las entidades de Identity (ApplicationUser, ApplicationRole, etc.) no heredan
/// de Entity&lt;Guid&gt; y son ignoradas correctamente.
/// </summary>
public sealed class DomainEventDispatcherInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcherInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is not null)
            await DispatchDomainEventsAsync(eventData.Context, ct);

        return await base.SavingChangesAsync(eventData, result, ct);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken ct)
    {
        // Filtrar SOLO entidades de dominio que heredan de Entity<Guid>.
        // OfType<Entity<Guid>>() excluye ApplicationUser y demás entidades de Identity.
        var aggregatesConEventos = context.ChangeTracker
            .Entries<object>()
            .Select(e => e.Entity)
            .OfType<Entity<Guid>>()
            .Where(e => e.DomainEvents.Any())
            .ToList();

        if (!aggregatesConEventos.Any())
            return;

        var domainEvents = aggregatesConEventos
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Limpiar antes de despachar para evitar re-despacho si un handler
        // provoca otro SaveChanges dentro de la misma transacción.
        aggregatesConEventos.ForEach(a => a.ClearDomainEvents());

        // Publicar cada event envolviéndolo en DomainEventNotification<T>
        // para que MediatR pueda resolverlo correctamente.
        // El dominio no referencia MediatR — Infrastructure actúa como adaptador.
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());

            var notification = (INotification)Activator.CreateInstance(
                notificationType, domainEvent)!;

            await _publisher.Publish(notification, ct);
        }
    }
}
