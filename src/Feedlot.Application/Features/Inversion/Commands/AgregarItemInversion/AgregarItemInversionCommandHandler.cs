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
    private readonly IUnitOfWork _unitOfWork;

    public AgregarItemInversionCommandHandler(
        IEtapaInversionRepository etapaRepo,
        IUnitOfWork unitOfWork)
    {
        _etapaRepo = etapaRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AgregarItemInversionCommand request,
        CancellationToken ct)
    {
        var etapa = await _etapaRepo.ObtenerPorIdAsync(request.EtapaId, ct);
        if (etapa is null)
            return Result<Guid>.Failure("Etapa de inversión no encontrada.");

        if (!Enum.TryParse<EstadoItemInversion>(request.Estado, ignoreCase: true, out var estado))
            return Result<Guid>.Failure("Estado de ítem inválido. Use 'OK' o 'Pendiente'.");

        var costo = Dinero.Crear(request.Monto, request.Moneda);

        var item = etapa.AgregarItem(request.Producto, costo, request.Observacion, estado, request.PorcentajeAvance);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(item.Id);
    }
}
