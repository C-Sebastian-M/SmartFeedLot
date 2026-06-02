using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.AvanzarEstadoCamada;

/// <summary>
/// Avanza el estado de una camada: Preceba → Ceba → Vendida.
/// </summary>
public sealed record AvanzarEstadoCamadaCommand(
    Guid MarranaId,
    Guid CamadaId,
    string AccionEstado  // "AvanzarCeba" | "MarcarVendida"
) : ICommand;

public sealed class AvanzarEstadoCamadaCommandHandler
    : IRequestHandler<AvanzarEstadoCamadaCommand, Result>
{
    private readonly IMarranaRepository _repo;

    public AvanzarEstadoCamadaCommandHandler(IMarranaRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(AvanzarEstadoCamadaCommand request, CancellationToken ct)
    {
        var marrana = await _repo.ObtenerPorIdAsync(request.MarranaId, ct);
        if (marrana is null)
            return Result.NotFound($"No se encontró la marrana {request.MarranaId}.");

        var camada = marrana.Camadas.FirstOrDefault(c => c.Id == request.CamadaId);
        if (camada is null)
            return Result.NotFound($"No se encontró la camada {request.CamadaId}.");

        switch (request.AccionEstado)
        {
            case "AvanzarCeba":
                camada.AvanzarACeba();
                break;
            case "MarcarVendida":
                camada.MarcarVendida();
                break;
            default:
                return Result.Failure($"Acción de estado inválida: {request.AccionEstado}. Use 'AvanzarCeba' o 'MarcarVendida'.");
        }

        return Result.Success();
    }
}
