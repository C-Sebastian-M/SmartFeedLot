using FluentValidation;

namespace Feedlot.Application.Features.Operacion.Commands.CrearEmpleado;

public sealed class CrearEmpleadoCommandValidator : AbstractValidator<CrearEmpleadoCommand>
{
    public CrearEmpleadoCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PagoMensual).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}
