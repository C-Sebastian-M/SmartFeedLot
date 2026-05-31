using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Feedlot.Infrastructure.Persistence;

/// <summary>
/// Siembra de DATOS DE DEMOSTRACIÓN basada en el archivo real
/// "PRESUPUESTO CEBA DE GANADO.xlsx" (hojas SEGUIMIENTO GANADO,
/// COSTOS Y GASTOS MENSUALES y AMORTIZ CREDITO).
///
/// Características:
/// - IDEMPOTENTE: solo siembra si la tabla de animales está vacía.
/// - SOLO EN DESARROLLO: se ejecuta únicamente cuando el entorno es Development,
///   salvo override explícito con la variable de entorno SEED_DEMO_DATA=true/false.
///
/// Usa exclusivamente los factory methods del dominio (Crear/Registrar), nunca
/// los constructores, para respetar las invariantes de cada agregado.
///
/// Se invoca desde DatabaseInitializer DESPUÉS de sembrar categorías y socios,
/// porque los movimientos financieros referencian esas categorías por Id.
/// </summary>
public static class SeedDataDemo
{
    public static async Task SeedAsync(FeedlotDbContext context, ILogger logger)
    {
        if (!DemoSeedHabilitado())
            return;

        if (await context.Animals.AnyAsync())
        {
            logger.LogInformation("SeedDataDemo: ya existen animales, se omite la siembra de demo.");
            return;
        }

        logger.LogInformation("SeedDataDemo: sembrando datos de demo desde el Excel de ceba...");

        var lote = await SembrarGanadoAsync(context);
        await SembrarProveedoresYCompradoresAsync(context);
        await SembrarMovimientosFinancierosAsync(context, lote.Id);
        await SembrarPrestamoAsync(context);

        await context.SaveChangesAsync();
        logger.LogInformation("SeedDataDemo: siembra de datos de demo completada.");
    }

    // -------------------------------------------------------------------------
    // 1. Lote + animales reales (hoja SEGUIMIENTO GANADO) con sus pesajes
    //    y algunos eventos sanitarios / vacunas.
    // -------------------------------------------------------------------------
    private static async Task<Lote> SembrarGanadoAsync(FeedlotDbContext context)
    {
        var lote = Lote.Crear("L-CEBA-2024", "Ceba intensiva natural 2024", capacidadMaxima: 15);
        lote.Activar();
        context.Lotes.Add(lote);

        // precioCompra = pesoInicial * costo/kg ("Costo compra" del Excel)
        var defs = new List<AnimalDef>
        {
            new("Levito",   Sexo.Macho,  new DateOnly(2024, 8, 9),  185m, 7500m,  null,
                new() { (new DateOnly(2024,10,22),190m), (new DateOnly(2024,12,13),205m),
                        (new DateOnly(2025,6,9),240m),   (new DateOnly(2025,12,4),296m),
                        (new DateOnly(2026,4,8),296m) }),

            new("Pepe",     Sexo.Macho,  new DateOnly(2024, 8, 12), 195m, 7000m,  null,
                new() { (new DateOnly(2024,12,13),215m), (new DateOnly(2025,6,9),234m),
                        (new DateOnly(2025,12,4),260m),  (new DateOnly(2026,4,8),260m) }),

            new("Kike",     Sexo.Macho,  new DateOnly(2025, 6, 29), 300m, 7000m,  null,
                new() { (new DateOnly(2025,12,4),360m), (new DateOnly(2026,4,8),360m) }),

            new("Arandano", Sexo.Macho,  new DateOnly(2024, 10, 22), 36m, 7000m,  new DateOnly(2024,10,21),
                new() { (new DateOnly(2025,6,9),180m), (new DateOnly(2025,12,4),190m),
                        (new DateOnly(2026,4,15),240m) }),

            new("Resabios", Sexo.Macho,  new DateOnly(2025, 11, 23), 180m, 10000m, null,
                new() { (new DateOnly(2025,12,4),172m), (new DateOnly(2026,4,15),192m) }),

            new("Tono",     Sexo.Macho,  new DateOnly(2025, 11, 23), 190m, 10526m, null,
                new() { (new DateOnly(2025,12,4),190m), (new DateOnly(2026,4,15),230m) }),

            new("Rolo",     Sexo.Macho,  new DateOnly(2026, 1, 2),   98m, 10408m, new DateOnly(2025,9,1),
                new()),

            new("Gasela",   Sexo.Hembra, new DateOnly(2026, 1, 2),   86m, 9302m,  new DateOnly(2025,9,1),
                new()),

            new("Canelo",   Sexo.Macho,  new DateOnly(2026, 5, 5),  194m, 10100m, null,
                new()),
        };

        var animales = new List<Animal>();
        foreach (var d in defs)
        {
            var precio = Math.Round(d.PesoInicial * d.CostoKg, 0);
            var animal = Animal.Registrar(
                codigoIdentificacion: CodigoIdentificacion.Crear($"AN-{Slug(d.Nombre)}"),
                nombre: d.Nombre,
                numeroArete: $"AR-{Slug(d.Nombre)}",
                sexo: d.Sexo,
                raza: "Criollo",
                fechaNacimiento: d.FechaNacimiento,
                pesoIngreso: Peso.Crear(d.PesoInicial),
                precioCompra: Dinero.Crear(precio, "COP"),
                fechaIngreso: d.FechaIngreso);

            foreach (var (fecha, peso) in d.Pesajes)
                animal.RegistrarPesaje(fecha, Peso.Crear(peso));

            animales.Add(animal);
        }

        context.Animals.AddRange(animales);

        // Asociar cada animal al lote activo (respeta capacidad e invariantes del lote).
        foreach (var a in animales)
            lote.AgregarAnimal(a.Id, a.FechaIngreso, MotivoMovimiento.IngresoInicial);

        // --- Eventos sanitarios / vacunas (Excel: baño, desparasitante, vitamina, vacuna) ---
        var levito = animales.First(a => a.Nombre == "Levito");
        var tono = animales.First(a => a.Nombre == "Tono");
        var pepe = animales.First(a => a.Nombre == "Pepe");

        // Vacuna con próxima dosis FUTURA -> ejercita la alerta de vacunas próximas.
        levito.RegistrarEventoSanitario(
            fechaEvento: new DateOnly(2024, 12, 13),
            diagnostico: "Vacuna aftosa",
            descripcion: "Ciclo de vacunación bovina obligatorio",
            severidad: SeveridadEvento.Leve,
            tratamiento: "Aplicación intramuscular",
            tipoEvento: "Vacuna",
            proximaDosis: DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1),
            responsable: "Operador");

        tono.RegistrarEventoSanitario(
            fechaEvento: new DateOnly(2025, 11, 25),
            diagnostico: "Desparasitación y vitaminas",
            descripcion: "Purgante y complejo vitamínico al ingreso",
            severidad: SeveridadEvento.Leve,
            tratamiento: "Dosis única",
            tipoEvento: "Tratamiento");

        pepe.RegistrarEventoSanitario(
            fechaEvento: new DateOnly(2024, 8, 15),
            diagnostico: "Baño garrapaticida",
            descripcion: "Control de ectoparásitos al ingreso",
            severidad: SeveridadEvento.Leve,
            tratamiento: "Inmersión",
            tipoEvento: "Tratamiento");

        await Task.CompletedTask;
        return lote;
    }

    // -------------------------------------------------------------------------
    // 2. Proveedores y compradores (Excel: Madre, Planeta Rica, SUBAGAN)
    // -------------------------------------------------------------------------
    private static async Task SembrarProveedoresYCompradoresAsync(FeedlotDbContext context)
    {
        context.Proveedores.AddRange(
            Proveedor.Crear("Finca La Madre", "Alquiler de potreros y espacio", null, null),
            Proveedor.Crear("Planeta Rica", "Cortadora de pasto / picadora", null, null),
            Proveedor.Crear("Distribuidora Agro Insumos", "Sal, melaza, salvado, vitaminas", null, null));

        context.Compradores.AddRange(
            Comprador.Crear("SUBAGAN", "Subasta ganadera — comisión 3%", null, null),
            Comprador.Crear("Carnicería Local", "Venta directa por kg en pie", null, null));

        await Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // 3. Movimientos financieros mensuales (Excel: COSTOS Y GASTOS MENSUALES)
    //    Mano de obra fija ~200.000/mes, alquiler ~100.000/mes, alimentación.
    // -------------------------------------------------------------------------
    private static async Task SembrarMovimientosFinancierosAsync(FeedlotDbContext context, Guid loteId)
    {
        var categorias = await context.CategoriasGasto.ToListAsync();
        Guid Cat(string nombre) => categorias.First(c => c.Nombre == nombre).Id;

        var manoObra = Cat("Mano de Obra");
        var alquiler = Cat("Alquiler de Potrero");
        var alimento = Cat("Alimento y Melaza");

        var adminId = await context.Users
            .Where(u => u.Email == "admin@feedlot.com")
            .Select(u => u.Id)
            .FirstAsync();

        var movimientos = new List<MovimientoFinanciero>();
        for (int mes = 1; mes <= 5; mes++)
        {
            var fecha = new DateOnly(2026, mes, 1);
            movimientos.Add(MovimientoFinanciero.Registrar(
                fecha, fecha.Year, fecha.Month, manoObra, Dinero.Crear(200_000m, "COP"),
                OrigenFinanciero.Bovino, "Mano de obra mensual (manutención potreros y animales)",
                socioId: null, registradoPorId: adminId));
            movimientos.Add(MovimientoFinanciero.Registrar(
                fecha, fecha.Year, fecha.Month, alquiler, Dinero.Crear(100_000m, "COP"),
                OrigenFinanciero.Bovino, "Alquiler de espacio / potrero",
                socioId: null, registradoPorId: adminId));
        }

        // Una compra puntual de alimento (silo, melaza, sal) en marzo.
        var fechaAlimento = new DateOnly(2026, 3, 10);
        movimientos.Add(MovimientoFinanciero.Registrar(
            fechaAlimento, fechaAlimento.Year, fechaAlimento.Month, alimento, Dinero.Crear(57_200m, "COP"),
            OrigenFinanciero.Bovino, "Insumos de alimentación: melaza, sal, salvado",
            socioId: null, registradoPorId: adminId));

        context.MovimientosFinancieros.AddRange(movimientos);
        await Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // 4. Préstamo con tabla de amortización (Excel: AMORTIZ CREDITO)
    //    $20M, tasa 1.79% mensual, 12 cuotas -> cuota ~$1.868M.
    // -------------------------------------------------------------------------
    private static async Task SembrarPrestamoAsync(FeedlotDbContext context)
    {
        var prestamo = Prestamo.Crear(
            descripcion: "Crédito de capital de trabajo para compra de ganado",
            capital: Dinero.Crear(20_000_000m, "COP"),
            tasaMensual: 0.0179m,
            nCuotas: 12,
            fechaInicio: new DateOnly(2026, 1, 1));

        context.Prestamos.Add(prestamo);
        await Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Demo habilitado si SEED_DEMO_DATA está en true; deshabilitado si está en false;
    /// si no está definida, se habilita solo en entorno Development.
    /// </summary>
    private static bool DemoSeedHabilitado()
    {
        var flag = Environment.GetEnvironmentVariable("SEED_DEMO_DATA");
        if (!string.IsNullOrWhiteSpace(flag))
            return string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase);

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Normaliza el nombre a un código alfanumérico válido (mayúsculas, sin tildes/espacios).</summary>
    private static string Slug(string nombre)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in nombre.Trim().ToUpperInvariant())
        {
            if (char.IsLetterOrDigit(c) && c < 128) sb.Append(c);
            else if (c == 'Á') sb.Append('A');
            else if (c == 'É') sb.Append('E');
            else if (c == 'Í') sb.Append('I');
            else if (c == 'Ó') sb.Append('O');
            else if (c == 'Ú' || c == 'Ü') sb.Append('U');
            else if (c == 'Ñ') sb.Append('N');
        }
        return sb.Length >= 1 ? sb.ToString() : "X";
    }

    private sealed record AnimalDef(
        string Nombre,
        Sexo Sexo,
        DateOnly FechaIngreso,
        decimal PesoInicial,
        decimal CostoKg,
        DateOnly? FechaNacimiento,
        List<(DateOnly Fecha, decimal Peso)> Pesajes);
}
