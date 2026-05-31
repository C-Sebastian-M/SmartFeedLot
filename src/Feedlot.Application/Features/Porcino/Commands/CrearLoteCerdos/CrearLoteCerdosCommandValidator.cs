using FluentValidation;

namespace Feedlot.Application.Features.Porcino.Commands.CrearLoteCerdos;

public sealed class CrearLoteCerdosCommandValidator : AbstractValidator<CrearLoteCerdosCommand>
{
    public CrearLoteCerdosCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FechaInicio).NotEmpty();
        RuleFor(x => x.NAnimales).GreaterThan(0);
        RuleFor(x => x.PesoPromedioKg).GreaterThan(0);
        RuleFor(x => x.Ciclo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Moneda).Length(3).When(x => x.Moneda != null);
    }
}
