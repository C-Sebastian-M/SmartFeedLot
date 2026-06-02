using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Inversion.Commands.AgregarItemInversion;

public sealed record AgregarItemInversionCommand(
    Guid EtapaId,
    string Producto,
    decimal Monto,
    string Moneda,
    string? Observacion,
    string Estado,
    decimal PorcentajeAvance
) : ICommand<Guid>;
public sealed class AgregarItemInversionCommandValidator : AbstractValidator<AgregarItemInversionCommand>
{
    public AgregarItemInversionCommandValidator()
    {
        RuleFor(x => x.EtapaId).NotEmpty();
        RuleFor(x => x.Producto).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Monto).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
        RuleFor(x => x.Estado).Must(e => e is "OK" or "Pendiente").WithMessage("Estado debe ser 'OK' o 'Pendiente'.");
        RuleFor(x => x.PorcentajeAvance).InclusiveBetween(0, 100);
    }
}

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
