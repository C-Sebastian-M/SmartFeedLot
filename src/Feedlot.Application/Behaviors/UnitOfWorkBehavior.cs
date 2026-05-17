using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Behaviors;

/// <summary>
/// Pipeline Behavior que llama a IUnitOfWork.SaveChangesAsync() automáticamente
/// al finalizar cualquier Command exitoso.
/// 
/// Solo aplica a Commands (ICommand marker) — las Queries no deben
/// tener side effects sobre la base de datos.
/// 
/// Esto evita que cada Handler tenga que llamar _unitOfWork.SaveChangesAsync()
/// manualmente — separación de responsabilidades: el Handler orquesta el dominio,
/// el Behavior persiste.
/// 
/// Orden en el pipeline: ValidationBehavior → UnitOfWorkBehavior → LoggingBehavior → Handler
/// La transacción se abre implícitamente con EF Core y se confirma aquí.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Queries no necesitan persistencia. Las identificamos por convención de nombre.
        if (IsQuery(request))
            return await next();

        var response = await next();
        await _unitOfWork.SaveChangesAsync(ct);
        return response;
    }

    private static bool IsQuery(TRequest request)
        => typeof(TRequest).Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase);
}
