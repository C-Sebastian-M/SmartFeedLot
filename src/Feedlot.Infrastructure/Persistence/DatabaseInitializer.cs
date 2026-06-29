using Feedlot.Domain.Entities;
using Feedlot.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Feedlot.Infrastructure.Persistence;

/// <summary>
/// Inicializa la base de datos: aplica migraciones pendientes y siembra datos esenciales.
/// Se llama desde Program.cs al arranque de la aplicación.
///
/// Datos sembrados:
/// - Roles del sistema: Admin, Supervisor, Operador.
/// - Usuario administrador por defecto (credenciales en appsettings de Development).
///
/// NO se siembran datos de negocio (animales, lotes, categorías, socios, etc.):
/// la base arranca vacía y el usuario carga su propia información.
///
/// Es idempotente — puede ejecutarse múltiples veces sin duplicar datos.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FeedlotDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FeedlotDbContext>>();

        try
        {
            logger.LogInformation("Feedlot — Aplicando migraciones pendientes...");
            await context.Database.MigrateAsync();

            // Solo se siembran usuarios (roles + admin). El resto de datos
            // (catálogos y negocio) los crea el usuario desde la aplicación.
            await SembrarRolesAsync(context);
            await SembrarUsuarioAdminAsync(context, scope.ServiceProvider);
            await SembrarModulosAsync(context);

            logger.LogInformation("Feedlot — Base de datos inicializada correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Feedlot — Error al inicializar la base de datos.");
            throw;
        }
    }

    /// <summary>
    /// Siembra el catálogo de módulos del sistema. Idempotente: solo agrega los
    /// que falten, sin tocar el estado (Activo) de los que ya existen.
    /// </summary>
    private static async Task SembrarModulosAsync(FeedlotDbContext context)
    {
        // (clave, nombre, activoPorDefecto, orden)
        var catalogo = new (string Clave, string Nombre, bool Activo, int Orden)[]
        {
            ("animales",        "Animales",          true,  1),
            ("lotes",           "Lotes",             true,  2),
            ("operacion",       "Campo (Operación)", true,  3),
            ("porcino",         "Porcino",           false, 4),
            ("finanzas",        "Movimientos",       true,  5),
            ("prestamos",       "Préstamos",         true,  6),
            ("inversion",       "Inversión",         true,  7),
            ("costos",          "Costeo",            true,  8),
            ("ventas",          "Ventas",            true,  9),
            ("compras",         "Compras",           true, 10),
            ("proveedores",     "Proveedores",       true, 11),
            ("compradores",     "Compradores",       true, 12),
            ("precios-mercado", "Precios de Mercado",true, 13),
            ("analitica",       "Analítica",         true, 14),
            ("alertas",         "Alertas",           true, 15),
        };

        var clavesExistentes = await context.Set<ModuloSistema>()
            .Select(m => m.Clave)
            .ToListAsync();

        var faltantes = catalogo
            .Where(c => !clavesExistentes.Contains(c.Clave))
            .Select(c => ModuloSistema.Crear(c.Clave, c.Nombre, c.Activo, c.Orden))
            .ToList();

        if (faltantes.Count > 0)
        {
            await context.Set<ModuloSistema>().AddRangeAsync(faltantes);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SembrarRolesAsync(FeedlotDbContext context)
    {
        var rolesExistentes = await context.Roles.Select(r => r.Nombre).ToListAsync();

        var rolesFaltantes = new[]
        {
            new ApplicationRole
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Nombre = "Admin",
                Descripcion = "Acceso total al sistema. Gestión de usuarios y configuración."
            },
            new ApplicationRole
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Nombre = "Supervisor",
                Descripcion = "Acceso a reportes, analítica y gestión de lotes."
            },
            new ApplicationRole
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Nombre = "Operador",
                Descripcion = "Registro de pesajes, consumo y eventos sanitarios."
            }
        }.Where(r => !rolesExistentes.Contains(r.Nombre)).ToList();

        if (rolesFaltantes.Count > 0)
        {
            await context.Roles.AddRangeAsync(rolesFaltantes);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SembrarUsuarioAdminAsync(
        FeedlotDbContext context,
        IServiceProvider sp)
    {
        const string adminEmail = "admin@feedlot.com";

        var adminExiste = await context.Users
            .AnyAsync(u => u.Email == adminEmail);

        if (adminExiste) return;

        var authService = sp.GetRequiredService<AuthService>();
        await authService.RegistrarAsync(
            email: adminEmail,
            nombreCompleto: "Administrador del Sistema",
            password: "Admin123!",   // ← Cambiar en producción via variable de entorno.
            rolNombre: "Admin");
    }
}
