using FluentValidation;

namespace Feedlot.Application.Features.Lotes.Commands.CrearLote;

public sealed class CrearLoteCommandValidator : AbstractValidator<CrearLoteCommand>
{
    public CrearLoteCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del lote es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.CapacidadMaxima)
            .GreaterThan(0).WithMessage("La capacidad máxima debe ser mayor a cero.")
            .LessThanOrEqualTo(10000).WithMessage("La capacidad máxima no puede superar 10.000 animales.");
    }
}
