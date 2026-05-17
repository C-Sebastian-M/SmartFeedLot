using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Feedlot.Application.Behaviors;

/// <summary>
/// Pipeline Behavior que registra en Serilog cada request con su duración.
/// Alerta si un Handler tarda más de 500ms — posible problema de rendimiento.
/// 
/// Produce logs estructurados: { RequestName, Duration, UserId }
/// que Serilog puede enrutar a Seq, Elasticsearch, etc.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "Feedlot — Iniciando {RequestName}: {@Request}",
            requestName, request);

        try
        {
            var response = await next();
            sw.Stop();

            if (sw.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning(
                    "Feedlot — Request lento detectado: {RequestName} tomó {ElapsedMs}ms. {@Request}",
                    requestName, sw.ElapsedMilliseconds, request);
            }
            else
            {
                _logger.LogInformation(
                    "Feedlot — {RequestName} completado en {ElapsedMs}ms",
                    requestName, sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Feedlot — Error en {RequestName} tras {ElapsedMs}ms. {@Request}",
                requestName, sw.ElapsedMilliseconds, request);
            throw;
        }
    }
}
