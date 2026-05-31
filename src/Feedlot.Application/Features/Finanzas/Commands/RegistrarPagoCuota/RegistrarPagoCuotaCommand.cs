using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.RegistrarPagoCuota;

public sealed record RegistrarPagoCuotaCommand(
    Guid PrestamoId,
    Guid CuotaId,
    DateOnly FechaPago
) : IRequest<Result>;

public sealed class RegistrarPagoCuotaCommandHandler
    : IRequestHandler<RegistrarPagoCuotaCommand, Result>
{
    private readonly IPrestamoRepository _prestamoRepo;
    private readonly IUnitOfWork _uow;

    public RegistrarPagoCuotaCommandHandler(IPrestamoRepository prestamoRepo, IUnitOfWork uow)
    {
        _prestamoRepo = prestamoRepo;
        _uow = uow;
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
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
