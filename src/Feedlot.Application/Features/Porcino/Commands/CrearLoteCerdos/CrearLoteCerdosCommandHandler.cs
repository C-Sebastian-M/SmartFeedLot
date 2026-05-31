using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.CrearLoteCerdos;

public sealed class CrearLoteCerdosCommandHandler : IRequestHandler<CrearLoteCerdosCommand, Result<Guid>>
{
    private readonly ILoteCerdosRepository _repo;
    private readonly IUnitOfWork _uow;
    public CrearLoteCerdosCommandHandler(ILoteCerdosRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result<Guid>> Handle(CrearLoteCerdosCommand request, CancellationToken ct)
    {
        Dinero? precioVentaKg = null;
        if (request.PrecioVentaKg.HasValue)
            precioVentaKg = Dinero.Crear(request.PrecioVentaKg.Value, request.Moneda ?? "COP");

        var lote = LoteCerdos.Crear(request.Codigo, request.FechaInicio, request.NAnimales,
            request.PesoPromedioKg, request.Ciclo, request.CamadaId, precioVentaKg);
        await _repo.AgregarAsync(lote, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(lote.Id);
    }
}
