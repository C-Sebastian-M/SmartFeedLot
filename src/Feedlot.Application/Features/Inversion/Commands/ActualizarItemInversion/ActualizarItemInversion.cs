using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Inversion.Commands.ActualizarItemInversion;

public sealed record ActualizarItemInversionCommand(
    Guid ItemId,
    string Producto,
    decimal Monto,
    string Moneda,
    string? Observacion,
    string Estado,
    decimal PorcentajeAvance
) : ICommand;
public sealed class ActualizarItemInversionCommandValidator : AbstractValidator<ActualizarItemInversionCommand>
{
    public ActualizarItemInversionCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Producto).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Monto).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
        RuleFor(x => x.Estado).Must(e => e is "OK" or "Pendiente").WithMessage("Estado debe ser 'OK' o 'Pendiente'.");
        RuleFor(x => x.PorcentajeAvance).InclusiveBetween(0, 100);
    }
}

public sealed class ActualizarItemInversionCommandHandler
    : IRequestHandler<ActualizarItemInversionCommand, Result>
{
    private readonly IEtapaInversionRepository _etapaRepo;

    public ActualizarItemInversionCommandHandler(
        IEtapaInversionRepository etapaRepo)
    {
        _etapaRepo = etapaRepo;
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


        return Result.Success();
    }
}
