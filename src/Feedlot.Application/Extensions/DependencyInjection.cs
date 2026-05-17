using Feedlot.Application.Behaviors;
using Feedlot.Application.Mappings;
using Feedlot.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Feedlot.Application.Extensions;

/// <summary>
/// Extension method que registra todos los servicios de la Application Layer
/// en el contenedor de DI de ASP.NET Core.
/// 
/// Se llama desde Program.cs: builder.Services.AddApplicationServices();
/// 
/// Principio: la API no conoce los detalles internos de Application —
/// solo llama a este método. Encapsulamiento de la capa.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR: registra todos los Handlers, Notifications, etc. del assembly.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Pipeline de behaviors — el orden importa:
            // 1. Logging primero y último (wrappea todo).
            // 2. Validation antes del Handler.
            // 3. UnitOfWork al final, justo antes del Handler (post-handler).
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
        });

        // FluentValidation: registra todos los AbstractValidator<T> del assembly.
        services.AddValidatorsFromAssembly(assembly);

        // AutoMapper: registra todos los Profile del assembly.
        services.AddAutoMapper(cfg => { }, assembly);

        // Domain Services — ciclo de vida Scoped (por request HTTP).
        // Son stateless pero coordinan aggregates, por eso Scoped y no Singleton.
        services.AddScoped<AnimalLoteService>();
        services.AddScoped<IndicadorProductivoService>();

        return services;
    }
}
