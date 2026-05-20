using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Persistence;

/// <summary>
/// DbContext principal del sistema feedlot.
/// Implementa IUnitOfWork — SaveChangesAsync() es el commit de la transacción.
///
/// Decisiones de diseño:
/// - Cada Aggregate Root tiene su propio DbSet. Las entidades internas
///   (Pesaje, EventoSanitario, AnimalLote, RacionIngrediente) se configuran
///   a través del aggregate, no con DbSet propio — respeta los límites de DDD.
/// - Las configuraciones Fluent API viven en clases separadas (IEntityTypeConfiguration<T>)
///   para mantener el DbContext limpio y enfocado.
/// - El interceptor DomainEventDispatcherInterceptor despacha events automáticamente.
/// </summary>
public sealed partial class FeedlotDbContext : DbContext, IUnitOfWork
{
    private readonly DomainEventDispatcherInterceptor _domainEventInterceptor;

    public FeedlotDbContext(
        DbContextOptions<FeedlotDbContext> options,
        DomainEventDispatcherInterceptor domainEventInterceptor)
        : base(options)
    {
        _domainEventInterceptor = domainEventInterceptor;
    }

    // Aggregate Roots — cada uno es un DbSet independiente.
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<ConsumoAlimenticio> Consumos => Set<ConsumoAlimenticio>();
    public DbSet<Racion> Raciones => Set<Racion>();
    public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();

    // Entidades internas accesibles para queries (pero creadas solo por los aggregates).
    public DbSet<Pesaje> Pesajes => Set<Pesaje>();
    public DbSet<AnimalLote> AnimalesLote => Set<AnimalLote>();
    public DbSet<EventoSanitario> EventosSanitarios => Set<EventoSanitario>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_domainEventInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica automáticamente todas las IEntityTypeConfiguration<T>
        // del assembly — convención sobre configuración.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeedlotDbContext).Assembly);

        // Schema explícito para organización en PostgreSQL.
        modelBuilder.HasDefaultSchema("feedlot");

        base.OnModelCreating(modelBuilder);
    }
}
