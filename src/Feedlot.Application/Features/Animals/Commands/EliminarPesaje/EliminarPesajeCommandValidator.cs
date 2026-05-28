using FluentValidation;

namespace Feedlot.Application.Features.Animals.Commands.EliminarPesaje;

public sealed class EliminarPesajeCommandValidator
    : AbstractValidator<EliminarPesajeCommand>
{
    public EliminarPesajeCommandValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty().WithMessage("El ID del animal es requerido.");

        RuleFor(x => x.PesajeId)
            .NotEmpty().WithMessage("El ID del pesaje es requerido.");
    }
}
