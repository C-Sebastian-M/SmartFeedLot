using FluentValidation;

namespace Feedlot.Application.Features.Inversion.Commands.ActualizarItemInversion;

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
