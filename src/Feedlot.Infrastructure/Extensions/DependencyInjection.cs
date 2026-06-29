using Feedlot.Domain.Interfaces;
using Feedlot.Domain.Services;
using Feedlot.Infrastructure.Identity;
using Feedlot.Infrastructure.Persistence;
using Feedlot.Infrastructure.Persistence.Interceptors;
using Feedlot.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Feedlot.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Interceptor — Scoped porque necesita IPublisher (MediatR).
        services.AddScoped<DomainEventDispatcherInterceptor>();

        // EF Core con PostgreSQL.
        // Prioridad de connection string:
        //   1. Variable de entorno ConnectionStrings__DefaultConnection (producción Railway/Render)
        //   2. appsettings.Development.json (desarrollo local)
        services.AddDbContext<FeedlotDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<DomainEventDispatcherInterceptor>();

            var connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la connection string 'DefaultConnection'.");

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(FeedlotDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });

            options.AddInterceptors(interceptor);

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // IUnitOfWork → FeedlotDbContext (mismo Scoped instance por request).
        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<FeedlotDbContext>());

        // ── Repositorios ─────────────────────────────────────────────────────
        // Core bovino
        services.AddScoped<IAnimalRepository, AnimalRepository>();
        services.AddScoped<ILoteRepository, LoteRepository>();
        services.AddScoped<IConsumoAlimenticioRepository, ConsumoAlimenticioRepository>();
        services.AddScoped<ICostoOperativoRepository, CostoOperativoRepository>();

        // Nutrición
        services.AddScoped<IRacionRepository, RacionRepository>();
        services.AddScoped<IIngredienteRepository, IngredienteRepository>();

        // Financiero
        services.AddScoped<IMovimientoFinancieroRepository, MovimientoFinancieroRepository>();
        services.AddScoped<ICategoriaGastoRepository, CategoriaGastoRepository>();
        services.AddScoped<IPrestamoRepository, PrestamoRepository>();

        // Comercial
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<ICompradorRepository, CompradorRepository>();
        services.AddScoped<ICompraRepository, CompraRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();

        // Operaciones
        services.AddScoped<ISocioRepository, SocioRepository>();
        services.AddScoped<IAporteSocioRepository, AporteSocioRepository>();
        services.AddScoped<IPotreroRepository, PotreroRepository>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();

        // Cultivos y silos
        services.AddScoped<ICultivoCaniaRepository, CultivoCaniaRepository>();
        services.AddScoped<ILoteSiloRepository, LoteSiloRepository>();

        // Inversión
        services.AddScoped<IEtapaInversionRepository, EtapaInversionRepository>();

        // Presupuesto
        services.AddScoped<IPresupuestoRepository, PresupuestoRepository>();

        // Porcinos
        services.AddScoped<IMarranaRepository, MarranaRepository>();
        services.AddScoped<ILoteCerdosRepository, LoteCerdosRepository>();

        // Mercado
        services.AddScoped<IPrecioMercadoRepository, PrecioMercadoRepository>();

        // Subagan
        services.AddScoped<ISubaganEventoRepository, SubaganEventoRepository>();

        // Configuración / administración
        services.AddScoped<IModuloSistemaRepository, ModuloSistemaRepository>();

        // ── Domain Services ───────────────────────────────────────────────────
        services.AddScoped<IndicadorProductivoService>();

        // ── Servicios HTTP externos ───────────────────────────────────────────
        // SubaganHttpService es Transient porque crea su propio HttpClient
        // con CookieContainer por sesión — no es thread-safe para compartir.
        services.AddTransient<Feedlot.Application.Services.ISubaganHttpService,
            Feedlot.Infrastructure.Services.SubaganHttpService>();

        // ── Identity ──────────────────────────────────────────────────────────
        services.AddScoped<JwtTokenService>();
        services.AddScoped<AuthService>();

        return services;
    }
}
