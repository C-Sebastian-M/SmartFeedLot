using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Feedlot.Infrastructure.Persistence;

/// <summary>
/// DbContext principal del sistema feedlot.
/// 
/// CORRECCIÓN: el interceptor NO se registra aquí en OnConfiguring.
/// Se registra UNA SOLA VEZ desde DependencyInjection via AddInterceptors()
/// en el DbContextOptions. Registrarlo en ambos lugares lo ejecutaba dos veces
/// por cada SaveChanges.
/// </summary>
public sealed partial class FeedlotDbContext : DbContext, IUnitOfWork
{
    public FeedlotDbContext(DbContextOptions<FeedlotDbContext> options)
        : base(options)
    {
    }

    // Aggregate Roots
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<ConsumoAlimenticio> Consumos => Set<ConsumoAlimenticio>();
    public DbSet<Racion> Raciones => Set<Racion>();
    public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();

    // Entidades internas accesibles para queries directas
    public DbSet<Pesaje> Pesajes => Set<Pesaje>();
    public DbSet<AnimalLote> AnimalesLote => Set<AnimalLote>();
    public DbSet<EventoSanitario> EventosSanitarios => Set<EventoSanitario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeedlotDbContext).Assembly);
        modelBuilder.HasDefaultSchema("feedlot");
        base.OnModelCreating(modelBuilder);
    }
}
