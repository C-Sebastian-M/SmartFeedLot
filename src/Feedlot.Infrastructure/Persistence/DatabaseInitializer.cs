using Feedlot.Infrastructure.Identity;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
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

            await SembrarRolesAsync(context);
            await SembrarUsuarioAdminAsync(context, scope.ServiceProvider);
            await SembrarCategoriasYSociosAsync(context);

            logger.LogInformation("Feedlot — Base de datos inicializada correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Feedlot — Error al inicializar la base de datos.");
            throw;
        }
    }

    private static async Task SembrarCategoriasYSociosAsync(FeedlotDbContext context)
    {
        // 1. Categorías de Gasto
        if (!await context.Set<CategoriaGasto>().AnyAsync())
        {
            var categorias = new List<CategoriaGasto>
            {
                CategoriaGasto.Crear("Mano de Obra", TipoCategoriaGasto.Operativo),
                CategoriaGasto.Crear("Gasolina y Combustibles", TipoCategoriaGasto.Indirecto),
                CategoriaGasto.Crear("Alquiler de Potrero", TipoCategoriaGasto.Indirecto),
                CategoriaGasto.Crear("Grama Fin (Matamaleza)", TipoCategoriaGasto.Indirecto),
                CategoriaGasto.Crear("Urea y Cal Agrícola", TipoCategoriaGasto.Indirecto),
                CategoriaGasto.Crear("Medicinas y Vacunas", TipoCategoriaGasto.Directo),
                CategoriaGasto.Crear("Alimento y Melaza", TipoCategoriaGasto.Directo),
                CategoriaGasto.Crear("Compra de Animales", TipoCategoriaGasto.Directo),
                CategoriaGasto.Crear("Inversión Infraestructura", TipoCategoriaGasto.Inversion),
                CategoriaGasto.Crear("Otros Gastos Generales", TipoCategoriaGasto.Indirecto)
            };
            await context.Set<CategoriaGasto>().AddRangeAsync(categorias);
        }

        // 2. Socios
        if (!await context.Set<Socio>().AnyAsync())
        {
            var socios = new List<Socio>
            {
                Socio.Crear("Estefania", 50.00m),
                Socio.Crear("Levir", 50.00m)
            };
            await context.Set<Socio>().AddRangeAsync(socios);
        }

        await context.SaveChangesAsync();
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
