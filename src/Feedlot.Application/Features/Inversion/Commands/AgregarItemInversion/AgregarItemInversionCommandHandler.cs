using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Inversion.Commands.AgregarItemInversion;

public sealed class AgregarItemInversionCommandHandler
    : IRequestHandler<AgregarItemInversionCommand, Result<Guid>>
{
    private readonly IEtapaInversionRepository _etapaRepo;

    public AgregarItemInversionCommandHandler(
        IEtapaInversionRepository etapaRepo)
    {
        _etapaRepo = etapaRepo;
    }

    public async Task<Result<Guid>> Handle(
        AgregarItemInversionCommand request,
        CancellationToken ct)
    {
        var etapa = await _etapaRepo.ObtenerPorIdSinTrackingAsync(request.EtapaId, ct);
        if (etapa is null)
            return Result<Guid>.NotFound("Etapa de inversión no encontrada.");

        if (!Enum.TryParse<EstadoItemInversion>(request.Estado, ignoreCase: true, out var estado))
            return Result<Guid>.Failure("Estado de ítem inválido. Use 'OK' o 'Pendiente'.");

        var costo = Dinero.Crear(request.Monto, request.Moneda);
        var item = etapa.AgregarItem(request.Producto, costo, request.Observacion, estado, request.PorcentajeAvance);

        _etapaRepo.AgregarItem(item);

        return Result<Guid>.Success(item.Id);
    }
}
