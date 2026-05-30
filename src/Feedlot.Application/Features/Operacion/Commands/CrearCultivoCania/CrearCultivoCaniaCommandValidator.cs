using FluentValidation;

namespace Feedlot.Application.Features.Operacion.Commands.CrearCultivoCania;

public sealed class CrearCultivoCaniaCommandValidator : AbstractValidator<CrearCultivoCaniaCommand>
{
    public CrearCultivoCaniaCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CallesTotales).GreaterThan(0);
    }
}
