using FluentValidation;

namespace Feedlot.Application.Features.Nutricion.Commands.CrearRacion;

public sealed class CrearRacionCommandValidator : AbstractValidator<CrearRacionCommand>
{
    public CrearRacionCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la ración es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede superar 150 caracteres.");

        RuleFor(x => x.CostoKg)
            .GreaterThan(0).WithMessage("El costo por kilogramo debe ser mayor a cero.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.ProteinaPct)
            .InclusiveBetween(0, 100)
            .WithMessage("El porcentaje de proteína debe estar entre 0 y 100.");

        RuleFor(x => x.EnergiaMcal)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La energía no puede ser negativa.");
    }
}
