using FluentValidation;
using Feedlot.Domain.Enums;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarEventoSanitario;

public sealed class RegistrarEventoSanitarioCommandValidator
    : AbstractValidator<RegistrarEventoSanitarioCommand>
{
    public RegistrarEventoSanitarioCommandValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty().WithMessage("El ID del animal es requerido.");

        RuleFor(x => x.FechaEvento)
            .NotEmpty().WithMessage("La fecha del evento es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha del evento no puede ser futura.");

        RuleFor(x => x.Diagnostico)
            .NotEmpty().WithMessage("El diagnóstico es requerido.")
            .MaximumLength(200).WithMessage("El diagnóstico no puede superar 200 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres.");

        RuleFor(x => x.Severidad)
            .NotEmpty().WithMessage("La severidad es requerida.")
            .Must(s => Enum.TryParse<SeveridadEvento>(s, ignoreCase: true, out _))
            .WithMessage("Severidad inválida. Valores válidos: Leve, Moderado, Grave, Critico.");

        RuleFor(x => x.Tratamiento)
            .MaximumLength(500)
            .WithMessage("El tratamiento no puede superar 500 caracteres.")
            .When(x => x.Tratamiento is not null);
    }
}
