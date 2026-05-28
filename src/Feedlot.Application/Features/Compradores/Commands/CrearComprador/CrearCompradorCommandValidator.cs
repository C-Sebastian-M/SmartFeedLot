using FluentValidation;

namespace Feedlot.Application.Features.Compradores.Commands.CrearComprador;

public sealed class CrearCompradorCommandValidator : AbstractValidator<CrearCompradorCommand>
{
    public CrearCompradorCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del comprador es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.Contacto)
            .MaximumLength(200).WithMessage("El contacto no puede superar 200 caracteres.")
            .When(x => x.Contacto is not null);

        RuleFor(x => x.Telefono)
            .MaximumLength(50).WithMessage("El teléfono no puede superar 50 caracteres.")
            .When(x => x.Telefono is not null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(200).WithMessage("El email no puede superar 200 caracteres.")
            .When(x => x.Email is not null);
    }
}
