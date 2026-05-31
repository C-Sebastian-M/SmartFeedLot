using FluentValidation;

namespace Feedlot.Application.Features.Porcino.Commands.CrearMarrana;

public sealed class CrearMarranaCommandValidator : AbstractValidator<CrearMarranaCommand>
{
    public CrearMarranaCommandValidator()
    {
        RuleFor(x => x.Identificacion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FechaCompra).NotEmpty();
        RuleFor(x => x.Costo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}
