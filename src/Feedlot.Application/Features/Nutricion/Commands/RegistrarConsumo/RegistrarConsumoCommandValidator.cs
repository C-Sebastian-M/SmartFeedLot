using FluentValidation;

namespace Feedlot.Application.Features.Nutricion.Commands.RegistrarConsumo;

public sealed class RegistrarConsumoCommandValidator
    : AbstractValidator<RegistrarConsumoCommand>
{
    public RegistrarConsumoCommandValidator()
    {
        RuleFor(x => x.LoteId)
            .NotEmpty().WithMessage("El ID del lote es requerido.");

        RuleFor(x => x.RacionId)
            .NotEmpty().WithMessage("El ID de la ración es requerido.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha del consumo es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha del consumo no puede ser futura.");

        RuleFor(x => x.CantidadKg)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La cantidad de kilogramos no puede ser negativa.");

        RuleFor(x => x.CostoTotal)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El costo total no puede ser negativo.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.RegistradoPorId)
            .NotEmpty().WithMessage("El ID del usuario que registra es requerido.");
    }
}
