using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.RegistrarCorteCania;

public sealed class RegistrarCorteCaniaCommandHandler : IRequestHandler<RegistrarCorteCaniaCommand, Result<Guid>>
{
    private readonly ICultivoCaniaRepository _repo;
    public RegistrarCorteCaniaCommandHandler(ICultivoCaniaRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(RegistrarCorteCaniaCommand request, CancellationToken ct)
    {
        var cultivo = await _repo.ObtenerPorIdSinTrackingAsync(request.CultivoCaniaId, ct);
        if (cultivo is null) return Result<Guid>.NotFound($"Cultivo de caña {request.CultivoCaniaId} no encontrado.");

        var corte = cultivo.RegistrarCorte(request.Fecha, request.NCalles, request.Horas,
            request.BolsasSilo, request.Melaza, request.CostoJornal, request.Moneda);
        _repo.AgregarCorte(corte);
        return Result<Guid>.Success(corte.Id);
    }
}
