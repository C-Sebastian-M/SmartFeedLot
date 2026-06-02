using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.RegistrarPagoCuota;

public sealed record RegistrarPagoCuotaCommand(
    Guid PrestamoId,
    Guid CuotaId,
    DateOnly FechaPago
) : ICommand;

public sealed class RegistrarPagoCuotaCommandHandler
    : IRequestHandler<RegistrarPagoCuotaCommand, Result>
{
    private readonly IPrestamoRepository _prestamoRepo;

    public RegistrarPagoCuotaCommandHandler(IPrestamoRepository prestamoRepo)
    {
        _prestamoRepo = prestamoRepo;
    }

    public async Task<Result> Handle(RegistrarPagoCuotaCommand request, CancellationToken ct)
    {
        var prestamo = await _prestamoRepo.ObtenerPorIdAsync(request.PrestamoId, ct);
        if (prestamo is null)
            return Result.NotFound($"No se encontró el préstamo {request.PrestamoId}.");

        var cuota = prestamo.Cuotas.FirstOrDefault(c => c.Id == request.CuotaId);
        if (cuota is null)
            return Result.NotFound($"No se encontró la cuota {request.CuotaId}.");

        if (cuota.Pagada)
            return Result.Conflict("La cuota ya está marcada como pagada.");

        cuota.RegistrarPago(request.FechaPago);
        return Result.Success();
    }
}
