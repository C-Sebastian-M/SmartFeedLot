using FluentValidation;

namespace Feedlot.Application.Features.Operacion.Commands.CrearPotrero;

public sealed class CrearPotreroCommandValidator : AbstractValidator<CrearPotreroCommand>
{
    public CrearPotreroCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacidad).GreaterThan(0);
    }
}
