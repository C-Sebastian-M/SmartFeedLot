using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearLoteSilo;

public sealed class CrearLoteSiloCommandHandler : IRequestHandler<CrearLoteSiloCommand, Result<Guid>>
{
    private readonly ILoteSiloRepository _repo;
    public CrearLoteSiloCommandHandler(ILoteSiloRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(CrearLoteSiloCommand request, CancellationToken ct)
    {
        var costoUnitario = Dinero.Crear(request.CostoUnitario, request.Moneda);
        var lote = LoteSilo.Crear(request.FechaProduccion, request.Bolsas, costoUnitario, request.Observacion, request.CorteCaniaId);
        await _repo.AgregarAsync(lote, ct);
        return Result<Guid>.Success(lote.Id);
    }
}
