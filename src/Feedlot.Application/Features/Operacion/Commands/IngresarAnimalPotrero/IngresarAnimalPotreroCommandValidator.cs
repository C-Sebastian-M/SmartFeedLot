using FluentValidation;

namespace Feedlot.Application.Features.Operacion.Commands.IngresarAnimalPotrero;

public sealed class IngresarAnimalPotreroCommandValidator : AbstractValidator<IngresarAnimalPotreroCommand>
{
    public IngresarAnimalPotreroCommandValidator()
    {
        RuleFor(x => x.PotreroId).NotEmpty();
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.FechaEntrada).NotEmpty();
    }
}
