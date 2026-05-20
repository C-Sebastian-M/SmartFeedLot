using Feedlot.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Feedlot.API.Middlewares;

/// <summary>
/// Middleware global de manejo de excepciones.
/// Intercepta todas las excepciones no manejadas y las convierte en
/// respuestas HTTP con formato RFC 7807 (Problem Details).
///
/// Mapa de excepciones a códigos HTTP:
/// - ValidationException (FluentValidation) → 400 Bad Request
/// - DomainException (invariantes de negocio) → 422 Unprocessable Entity
/// - AnimalInactivoException → 422
/// - KeyNotFoundException / entidades no encontradas → 404 Not Found
/// - UnauthorizedAccessException → 401 Unauthorized
/// - Exception genérica → 500 Internal Server Error
///
/// Nunca expone stack traces en producción.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Feedlot — Validación fallida: {Errors}",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));

            await EscribirRespuestaAsync(context, StatusCodes.Status400BadRequest,
                "Datos inválidos",
                "Uno o más campos no pasaron la validación.",
                ex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Feedlot — Violación de regla de negocio: {Message}", ex.Message);

            await EscribirRespuestaAsync(context, StatusCodes.Status422UnprocessableEntity,
                "Regla de negocio violada", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Feedlot — Recurso no encontrado: {Message}", ex.Message);

            await EscribirRespuestaAsync(context, StatusCodes.Status404NotFound,
                "Recurso no encontrado", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Feedlot — Acceso no autorizado: {Message}", ex.Message);

            await EscribirRespuestaAsync(context, StatusCodes.Status401Unauthorized,
                "No autorizado", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feedlot — Error inesperado en {Path}", context.Request.Path);

            var detalle = _env.IsDevelopment()
                ? ex.ToString()
                : "Ocurrió un error interno. Por favor contacte al administrador.";

            await EscribirRespuestaAsync(context, StatusCodes.Status500InternalServerError,
                "Error interno del servidor", detalle);
        }
    }

    private static async Task EscribirRespuestaAsync(
        HttpContext context,
        int statusCode,
        string titulo,
        string detalle,
        Dictionary<string, string[]>? errores = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = detalle,
            Instance = context.Request.Path
        };

        if (errores is not null)
            problemDetails.Extensions["errors"] = errores;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
