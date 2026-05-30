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
    public DbSet<MovimientoFinanciero> MovimientosFinancieros => Set<MovimientoFinanciero>();
    public DbSet<CategoriaGasto> CategoriasGasto => Set<CategoriaGasto>();
    public DbSet<Socio> Socios => Set<Socio>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<Comprador> Compradores => Set<Comprador>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaItem> VentaItems => Set<VentaItem>();

    // Entidades internas accesibles para queries directas
    public DbSet<Pesaje> Pesajes => Set<Pesaje>();
    public DbSet<AnimalLote> AnimalesLote => Set<AnimalLote>();
    public DbSet<EventoSanitario> EventosSanitarios => Set<EventoSanitario>();
    public DbSet<CuotaAmortizacion> CuotasAmortizacion => Set<CuotaAmortizacion>();
    public DbSet<EtapaInversion> EtapasInversion => Set<EtapaInversion>();
    public DbSet<ItemInversion> ItemsInversion => Set<ItemInversion>();
    public DbSet<AporteSocio> AportesSocios => Set<AporteSocio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeedlotDbContext).Assembly);
        modelBuilder.HasDefaultSchema("feedlot");
        base.OnModelCreating(modelBuilder);
    }
}
