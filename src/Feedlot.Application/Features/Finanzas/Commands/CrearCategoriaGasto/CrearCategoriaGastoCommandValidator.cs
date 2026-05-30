using FluentValidation;
using Feedlot.Domain.Enums;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearCategoriaGasto;

public sealed class CrearCategoriaGastoCommandValidator
    : AbstractValidator<CrearCategoriaGastoCommand>
{
    public CrearCategoriaGastoCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la categoría es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo de categoría es requerido.")
            .Must(t => Enum.TryParse<TipoCategoriaGasto>(t, ignoreCase: true, out _))
            .WithMessage("Tipo de categoría inválido. Valores válidos: Directo, Indirecto, Operativo, Inversion.");
    }
}
