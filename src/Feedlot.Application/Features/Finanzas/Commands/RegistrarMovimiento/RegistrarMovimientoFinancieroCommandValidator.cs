using FluentValidation;
using Feedlot.Domain.Enums;

namespace Feedlot.Application.Features.Finanzas.Commands.RegistrarMovimiento;

public sealed class RegistrarMovimientoFinancieroCommandValidator
    : AbstractValidator<RegistrarMovimientoFinancieroCommand>
{
    public RegistrarMovimientoFinancieroCommandValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.PeriodoAnio)
            .InclusiveBetween(2000, 2100).WithMessage("El año del periodo no es válido.");

        RuleFor(x => x.PeriodoMes)
            .InclusiveBetween(1, 12).WithMessage("El mes del periodo debe estar entre 1 y 12.");

        RuleFor(x => x.CategoriaGastoId)
            .NotEmpty().WithMessage("La categoría de gasto es requerida.");

        RuleFor(x => x.Monto)
            .GreaterThan(0).WithMessage("El monto del movimiento debe ser mayor a cero.");

        RuleFor(x => x.Moneda)
            .NotEmpty().Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.Origen)
            .NotEmpty().WithMessage("El origen financiero es requerido.")
            .Must(o => Enum.TryParse<OrigenFinanciero>(o, ignoreCase: true, out _))
            .WithMessage("Origen financiero inválido. Valores válidos: Bovino, Porcino, Agricola, General.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");

        RuleFor(x => x.RegistradoPorId)
            .NotEmpty().WithMessage("El usuario que registra es requerido.");
    }
}
