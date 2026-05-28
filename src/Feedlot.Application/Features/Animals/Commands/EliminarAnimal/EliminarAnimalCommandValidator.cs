using FluentValidation;

namespace Feedlot.Application.Features.Animals.Commands.EliminarAnimal;

public sealed class EliminarAnimalCommandValidator
    : AbstractValidator<EliminarAnimalCommand>
{
    public EliminarAnimalCommandValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty().WithMessage("El ID del animal es requerido.");
    }
}
