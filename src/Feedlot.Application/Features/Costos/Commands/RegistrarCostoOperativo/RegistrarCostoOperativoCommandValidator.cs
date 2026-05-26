using FluentValidation;
using Feedlot.Domain.Enums;

namespace Feedlot.Application.Features.Costos.Commands.RegistrarCostoOperativo;

public sealed class RegistrarCostoOperativoCommandValidator
    : AbstractValidator<RegistrarCostoOperativoCommand>
{
    public RegistrarCostoOperativoCommandValidator()
    {
        RuleFor(x => x.LoteId)
            .NotEmpty().WithMessage("El ID del lote es requerido.");

        RuleFor(x => x.Categoria)
            .NotEmpty().WithMessage("La categoría es requerida.")
            .Must(c => Enum.TryParse<CategoriaCosto>(c, ignoreCase: true, out _))
            .WithMessage("Categoría inválida. Valores: ManoDeObra, CIF.");

        RuleFor(x => x.Concepto)
            .NotEmpty().WithMessage("El concepto es requerido.")
            .MaximumLength(200).WithMessage("Máximo 200 caracteres.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.Monto)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.Moneda)
            .NotEmpty().Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).When(x => x.Observaciones is not null);

        RuleFor(x => x.RegistradoPorId)
            .NotEmpty().WithMessage("El usuario que registra es requerido.");
    }
}
