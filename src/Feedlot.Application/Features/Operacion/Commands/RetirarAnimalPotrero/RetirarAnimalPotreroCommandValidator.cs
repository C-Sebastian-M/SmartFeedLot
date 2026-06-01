using FluentValidation;

namespace Feedlot.Application.Features.Operacion.Commands.RetirarAnimalPotrero;

public sealed class RetirarAnimalPotreroCommandValidator : AbstractValidator<RetirarAnimalPotreroCommand>
{
    public RetirarAnimalPotreroCommandValidator()
    {
        RuleFor(x => x.PotreroId).NotEmpty();
        RuleFor(x => x.EstanciaId).NotEmpty();
        RuleFor(x => x.FechaSalida)
            .NotEmpty()
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de salida no puede ser futura.");
    }
}
