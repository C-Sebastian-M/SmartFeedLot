using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearLoteSilo;

public sealed class CrearLoteSiloCommandHandler : IRequestHandler<CrearLoteSiloCommand, Result<Guid>>
{
    private readonly ILoteSiloRepository _repo;
    private readonly IUnitOfWork _uow;
    public CrearLoteSiloCommandHandler(ILoteSiloRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result<Guid>> Handle(CrearLoteSiloCommand request, CancellationToken ct)
    {
        var costoUnitario = Dinero.Crear(request.CostoUnitario, request.Moneda);
        var lote = LoteSilo.Crear(request.FechaProduccion, request.Bolsas, costoUnitario, request.Observacion, request.CorteCaniaId);
        await _repo.AgregarAsync(lote, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(lote.Id);
    }
}
