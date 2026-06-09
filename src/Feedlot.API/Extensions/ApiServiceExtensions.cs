using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Feedlot.API.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SmartFeedLot API",
                Version = "v1",
                Description = "Plataforma de gestión y analítica de feedlot bovino.",
            });

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
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Lee el secreto desde variable de entorno primero, luego appsettings.
        var secretKey =
            Environment.GetEnvironmentVariable("JwtSettings__SecretKey")
            ?? configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey no configurado.");

        var issuer =
            Environment.GetEnvironmentVariable("JwtSettings__Issuer")
            ?? configuration["JwtSettings:Issuer"]
            ?? "SmartFeedLot";

        var audience =
            Environment.GetEnvironmentVariable("JwtSettings__Audience")
            ?? configuration["JwtSettings:Audience"]
            ?? "SmartFeedLot-Frontend";

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
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddFeedlotCors(this IServiceCollection services)
    {
        // El origen de producción se configura via variable de entorno ALLOWED_ORIGINS.
        // Ejemplo: https://smartfeedlot.vercel.app,https://feedlot.app
        var allowedOrigins =
            Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("FeedlotFrontend", policy =>
            {
                var builder = policy
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();

                if (allowedOrigins.Length > 0)
                    builder.WithOrigins(allowedOrigins);
            });
        });

        return services;
    }
}
