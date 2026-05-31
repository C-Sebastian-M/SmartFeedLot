using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.AnularPagoCuota;

public sealed record AnularPagoCuotaCommand(
    Guid PrestamoId,
    Guid CuotaId
) : IRequest<Result>;

public sealed class AnularPagoCuotaCommandHandler
    : IRequestHandler<AnularPagoCuotaCommand, Result>
{
    private readonly IPrestamoRepository _prestamoRepo;
    private readonly IUnitOfWork _uow;

    public AnularPagoCuotaCommandHandler(IPrestamoRepository prestamoRepo, IUnitOfWork uow)
    {
        _prestamoRepo = prestamoRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(AnularPagoCuotaCommand request, CancellationToken ct)
    {
        var prestamo = await _prestamoRepo.ObtenerPorIdAsync(request.PrestamoId, ct);
        if (prestamo is null)
            return Result.NotFound($"No se encontró el préstamo {request.PrestamoId}.");

        var cuota = prestamo.Cuotas.FirstOrDefault(c => c.Id == request.CuotaId);
        if (cuota is null)
            return Result.NotFound($"No se encontró la cuota {request.CuotaId}.");

        if (!cuota.Pagada)
            return Result.Conflict("La cuota no está pagada, no hay nada que anular.");

        cuota.AnularPago();
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
