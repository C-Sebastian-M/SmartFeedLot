using FluentValidation;

namespace Feedlot.Application.Features.Porcino.Commands.RegistrarVentaLoteCerdos;

public sealed class RegistrarVentaLoteCerdosCommandValidator : AbstractValidator<RegistrarVentaLoteCerdosCommand>
{
    public RegistrarVentaLoteCerdosCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.FechaVenta).NotEmpty();
        RuleFor(x => x.PrecioVentaKg).GreaterThan(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}
