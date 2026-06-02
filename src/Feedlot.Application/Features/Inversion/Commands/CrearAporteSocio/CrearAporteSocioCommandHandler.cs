using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Inversion.Commands.CrearAporteSocio;

public sealed class CrearAporteSocioCommandHandler
    : IRequestHandler<CrearAporteSocioCommand, Result<Guid>>
{
    private readonly IAporteSocioRepository _aporteRepo;

    public CrearAporteSocioCommandHandler(
        IAporteSocioRepository aporteRepo)
    {
        _aporteRepo = aporteRepo;
    }

    public async Task<Result<Guid>> Handle(
        CrearAporteSocioCommand request,
        CancellationToken ct)
    {
        var monto = Dinero.Crear(request.Monto, request.Moneda);

        var aporte = AporteSocio.Crear(request.SocioId, request.ItemInversionId, monto);

        await _aporteRepo.AgregarAsync(aporte, ct);

        return Result<Guid>.Success(aporte.Id);
    }
}
