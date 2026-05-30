using FluentValidation;

namespace Feedlot.Application.Features.Operacion.Commands.CrearLoteSilo;

public sealed class CrearLoteSiloCommandValidator : AbstractValidator<CrearLoteSiloCommand>
{
    public CrearLoteSiloCommandValidator()
    {
        RuleFor(x => x.FechaProduccion).NotEmpty();
        RuleFor(x => x.Bolsas).GreaterThan(0);
        RuleFor(x => x.CostoUnitario).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
        RuleFor(x => x.Observacion).MaximumLength(500).When(x => x.Observacion != null);
    }
}
