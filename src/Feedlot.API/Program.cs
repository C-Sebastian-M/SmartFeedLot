using System.Text.Json.Serialization;
using Feedlot.API.Extensions;
using Feedlot.API.Middlewares;
using Feedlot.Application.Extensions;
using Feedlot.Infrastructure.Extensions;
using Feedlot.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Feedlot — Iniciando aplicación...");

    var builder = WebApplication.CreateBuilder(args);

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
                retainedFileCountLimit: 30);
    });

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // CORRECCIÓN 1: enums como strings para que el frontend reciba
            // "EnEngorde" en lugar de 0, alineado con los tipos TypeScript.
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            // CORRECCIÓN 2: camelCase en todas las propiedades JSON para que
            // el frontend reciba "codigoIdentificacion" no "CodigoIdentificacion".
            options.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;

            // CORRECCIÓN 3: DateOnly se serializa como "2024-01-15" (ISO 8601).
            // ASP.NET Core 8 ya lo soporta de forma nativa, sin converter extra.
        });

    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddFeedlotCors();
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    await DatabaseInitializer.InitializeAsync(app.Services);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartFeedLot API v1");
            options.RoutePrefix = string.Empty;
            options.DocumentTitle = "SmartFeedLot API";
        });
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} → {StatusCode} en {Elapsed:0.0000}ms";
    });

    // ExceptionHandlingMiddleware después de Serilog para que
    // capture las excepciones antes de que Serilog las loguee como 500.
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // CORRECCIÓN 5: en desarrollo no redirigir a HTTPS porque el frontend
    // Vite corre en HTTP. Solo redirigir en producción.
    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.UseCors("FeedlotFrontend");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Feedlot — Aplicación iniciada. Swagger en http://localhost:5000");
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

public partial class Program { }
