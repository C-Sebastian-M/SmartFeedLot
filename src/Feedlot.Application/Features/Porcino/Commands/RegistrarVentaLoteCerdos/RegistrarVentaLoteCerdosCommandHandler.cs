using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.RegistrarVentaLoteCerdos;

public sealed class RegistrarVentaLoteCerdosCommandHandler : IRequestHandler<RegistrarVentaLoteCerdosCommand, Result>
{
    private readonly ILoteCerdosRepository _repo;
    private readonly IUnitOfWork _uow;
    public RegistrarVentaLoteCerdosCommandHandler(ILoteCerdosRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result> Handle(RegistrarVentaLoteCerdosCommand request, CancellationToken ct)
    {
        var lote = await _repo.ObtenerPorIdAsync(request.LoteId, ct);
        if (lote is null)
            return Result.NotFound($"Lote de cerdos con Id {request.LoteId} no encontrado.");

        var precio = Dinero.Crear(request.PrecioVentaKg, request.Moneda);
        lote.RegistrarVenta(request.FechaVenta, precio);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
