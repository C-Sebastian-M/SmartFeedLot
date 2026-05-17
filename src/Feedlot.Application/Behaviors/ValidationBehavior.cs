using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Feedlot.Application.Behaviors;

/// <summary>
/// Pipeline Behavior de MediatR que ejecuta validaciones FluentValidation
/// ANTES de que el Handler procese el request.
/// 
/// Flujo: Request → ValidationBehavior → LoggingBehavior → Handler
/// 
/// Si hay errores de validación, lanza ValidationException inmediatamente
/// sin llegar al Handler. El ExceptionHandlingMiddleware de la API
/// lo convierte en HTTP 400 con detalle de los errores.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
