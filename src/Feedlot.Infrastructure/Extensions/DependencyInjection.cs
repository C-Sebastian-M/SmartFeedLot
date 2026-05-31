using Feedlot.Domain.Interfaces;
using Feedlot.Infrastructure.Identity;
using Feedlot.Infrastructure.Persistence;
using Feedlot.Infrastructure.Persistence.Interceptors;
using Feedlot.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Feedlot.Infrastructure.Extensions;

/// <summary>
/// Registro de todos los servicios de Infrastructure en el contenedor DI.
/// Llamado desde Program.cs: builder.Services.AddInfrastructureServices(configuration);
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Settings.
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        // Interceptor — Scoped porque necesita IPublisher (MediatR) que es Scoped.
        services.AddScoped<DomainEventDispatcherInterceptor>();

        // EF Core con PostgreSQL.
        services.AddDbContext<FeedlotDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<DomainEventDispatcherInterceptor>();

            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(FeedlotDbContext).Assembly.FullName);
                    // Retry policy para conexiones intermitentes en Docker.
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
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

        // Repositorios.
        services.AddScoped<IAnimalRepository, AnimalRepository>();
        services.AddScoped<ILoteRepository, LoteRepository>();
        services.AddScoped<IConsumoAlimenticioRepository, ConsumoAlimenticioRepository>();
        services.AddScoped<IRacionRepository, RacionRepository>();
        services.AddScoped<IIngredienteRepository, IngredienteRepository>();
        services.AddScoped<IMovimientoFinancieroRepository, MovimientoFinancieroRepository>();
        services.AddScoped<ICategoriaGastoRepository, CategoriaGastoRepository>();
        services.AddScoped<ISocioRepository, SocioRepository>();
        services.AddScoped<IPrestamoRepository, PrestamoRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<ICompraRepository, CompraRepository>();
        services.AddScoped<ICompradorRepository, CompradorRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IEtapaInversionRepository, EtapaInversionRepository>();
        services.AddScoped<IAporteSocioRepository, AporteSocioRepository>();
        services.AddScoped<IPotreroRepository, PotreroRepository>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<ICultivoCaniaRepository, CultivoCaniaRepository>();
        services.AddScoped<ILoteSiloRepository, LoteSiloRepository>();
        services.AddScoped<IPresupuestoRepository, PresupuestoRepository>();


        // Identity.
        services.AddScoped<JwtTokenService>();
        services.AddScoped<AuthService>();


        return services;
    }
}
