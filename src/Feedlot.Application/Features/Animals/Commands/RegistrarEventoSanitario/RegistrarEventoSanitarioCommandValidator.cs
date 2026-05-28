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

        RuleFor(x => x.TipoEvento)
            .Must(t => t is null || t is "Vacuna" or "Tratamiento" or "Otro")
            .WithMessage("Tipo de evento inválido. Valores válidos: Vacuna, Tratamiento, Otro.")
            .When(x => x.TipoEvento is not null);

        RuleFor(x => x.ProximaDosis)
            .Must(f => f > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La próxima dosis debe ser una fecha futura.")
            .When(x => x.ProximaDosis is not null);

        RuleFor(x => x.Responsable)
            .MaximumLength(200)
            .WithMessage("El responsable no puede superar 200 caracteres.")
            .When(x => x.Responsable is not null);
    }
}
