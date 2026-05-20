using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Feedlot.API.Extensions;

public static class ApiServiceExtensions
{
    /// <summary>
    /// Configura Swagger/OpenAPI con soporte para JWT Bearer authentication.
    /// El botón "Authorize" en Swagger UI permite probar endpoints protegidos.
    /// </summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SmartFeedLot API",
                Version = "v1",
                Description = "Plataforma de gestión y analítica de feedlot bovino. " +
                              "Monitoreo productivo, indicadores de eficiencia y rentabilidad.",
                Contact = new OpenApiContact
                {
                    Name = "SmartFeedLot",
                    Email = "dev@feedlot.com"
                }
            });

            // Definición del esquema JWT para Swagger UI.
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingresa el JWT token. Ejemplo: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Incluir comentarios XML de los controllers (si se habilita en el .csproj).
            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    /// <summary>Configura JWT Bearer authentication leyendo JwtSettings de appsettings.</summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey no configurado.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            ctx.Response.Headers.Append("Token-Expired", "true");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("SupervisorOrAdmin",
                policy => policy.RequireRole("Admin", "Supervisor"));
        });

        return services;
    }

    /// <summary>Configura CORS para el frontend React (localhost en desarrollo).</summary>
    public static IServiceCollection AddFeedlotCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("FeedlotFrontend", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5173",   // Vite dev server
                        "http://localhost:3000",
                        "https://feedlot.app")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
