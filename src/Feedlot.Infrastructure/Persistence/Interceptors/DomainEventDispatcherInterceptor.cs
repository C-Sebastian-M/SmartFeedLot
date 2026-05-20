using Feedlot.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Feedlot.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor de EF Core que despacha automáticamente los Domain Events
/// acumulados en los Aggregates ANTES de hacer commit a la base de datos.
///
/// Flujo:
/// 1. Handler modifica el Aggregate → el Aggregate llama RaiseDomainEvent().
/// 2. UnitOfWorkBehavior llama SaveChangesAsync().
/// 3. Este interceptor intercepta SavingChanges.
/// 4. Encuentra todos los Aggregates con events pendientes.
/// 5. Los despacha vía MediatR IPublisher (INotification handlers).
/// 6. Limpia los events del Aggregate.
/// 7. EF Core persiste los cambios.
///
/// Decisión de diseño: despachar ANTES del commit garantiza que los handlers
/// de eventos sean parte de la misma transacción. Si un handler falla,
/// todo hace rollback. Si se necesita eventual consistency, se despacharía DESPUÉS.
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
        // Recolectar todos los aggregates raíz con domain events pendientes.
        var aggregatesConEventos = context.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Extraer todos los events antes de despacharlos (evita colecciones modificadas).
        var domainEvents = aggregatesConEventos
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Limpiar events de los aggregates ANTES de despachar
        // para evitar re-despacho si un handler causa otro SaveChanges.
        aggregatesConEventos.ForEach(a => a.ClearDomainEvents());

        // Despachar cada event — MediatR enruta a los INotificationHandler<T> registrados.
        // En esta fase, los handlers de events se implementarán en fases posteriores.
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, ct);
        }
    }
}
