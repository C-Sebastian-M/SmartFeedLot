using Feedlot.API.Extensions;
using Feedlot.API.Middlewares;
using Feedlot.Application.Extensions;
using Feedlot.Infrastructure.Extensions;
using Feedlot.Infrastructure.Persistence;
using Serilog;

// ─── Serilog bootstrap logger ────────────────────────────────────────────────
// Logger temporal para capturar errores DURANTE la inicialización,
// antes de que el host esté disponible.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Feedlot — Iniciando aplicación...");

    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog definitivo ───────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} — {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/feedlot-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} — {Message:lj}{NewLine}{Exception}");
    });

    // ─── Servicios de Application y Infrastructure ────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // ─── Servicios de la API ──────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Serializar enums como strings en la API.
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
            // Serializar DateOnly correctamente.
            options.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;
        });

    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddFeedlotCors();
    builder.Services.AddSwaggerWithJwt();

    // Health checks básicos.
    builder.Services.AddHealthChecks();

    // ─── Build ────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ─── Inicializar base de datos ────────────────────────────────────────────
    await DatabaseInitializer.InitializeAsync(app.Services);

    // ─── Middleware pipeline ──────────────────────────────────────────────────
    // El orden importa en ASP.NET Core.

    // 1. Manejo global de excepciones — debe ser el primero.
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // 2. Swagger solo en desarrollo.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartFeedLot API v1");
            options.RoutePrefix = string.Empty; // Swagger en la raíz: http://localhost:5000/
            options.DocumentTitle = "SmartFeedLot API";
        });
    }

    // 3. Serilog request logging estructurado.
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} → {StatusCode} en {Elapsed:0.0000}ms";
    });

    app.UseHttpsRedirection();
    app.UseCors("FeedlotFrontend");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Feedlot — Aplicación iniciada. Swagger disponible en /");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Feedlot — La aplicación falló al iniciar.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Necesario para que xUnit pueda acceder al Program en tests de integración.
public partial class Program { }
