using FluentValidation;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearSocio;

public sealed class CrearSocioCommandValidator
    : AbstractValidator<CrearSocioCommand>
{
    public CrearSocioCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del socio es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Participacion)
            .InclusiveBetween(0, 100).WithMessage("La participación debe ser un porcentaje entre 0 y 100.");
    }
}
