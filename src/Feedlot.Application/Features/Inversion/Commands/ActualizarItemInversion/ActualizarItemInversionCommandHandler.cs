using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Inversion.Commands.ActualizarItemInversion;

public sealed class ActualizarItemInversionCommandHandler
    : IRequestHandler<ActualizarItemInversionCommand, Result>
{
    private readonly IEtapaInversionRepository _etapaRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarItemInversionCommandHandler(
        IEtapaInversionRepository etapaRepo,
        IUnitOfWork unitOfWork)
    {
        _etapaRepo = etapaRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ActualizarItemInversionCommand request,
        CancellationToken ct)
    {
        if (!Enum.TryParse<EstadoItemInversion>(request.Estado, ignoreCase: true, out var estado))
            return Result.Failure("Estado de ítem inválido. Use 'OK' o 'Pendiente'.");

        var costo = Dinero.Crear(request.Monto, request.Moneda);

        var etapas = await _etapaRepo.ObtenerTodosAsync(ct);
        var item = etapas.SelectMany(e => e.Items).FirstOrDefault(i => i.Id == request.ItemId);

        if (item is null)
            return Result.Failure("Ítem de inversión no encontrado.");

        item.Actualizar(request.Producto, costo, request.Observacion, estado, request.PorcentajeAvance);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
