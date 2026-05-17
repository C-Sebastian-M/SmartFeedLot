using FluentValidation;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarPesaje;

public sealed class RegistrarPesajeCommandValidator
    : AbstractValidator<RegistrarPesajeCommand>
{
    public RegistrarPesajeCommandValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty().WithMessage("El ID del animal es requerido.");

        RuleFor(x => x.FechaPesaje)
            .NotEmpty().WithMessage("La fecha del pesaje es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha del pesaje no puede ser futura.");

        RuleFor(x => x.PesoKg)
            .GreaterThan(0).WithMessage("El peso debe ser mayor a cero.")
            .LessThan(2000).WithMessage("El peso parece inválido (máx 2000 kg).");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden superar 500 caracteres.")
            .When(x => x.Observaciones is not null);
    }
}
