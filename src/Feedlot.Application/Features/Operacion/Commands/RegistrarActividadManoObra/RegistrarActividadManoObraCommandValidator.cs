using FluentValidation;

namespace Feedlot.Application.Features.Operacion.Commands.RegistrarActividadManoObra;

public sealed class RegistrarActividadManoObraCommandValidator : AbstractValidator<RegistrarActividadManoObraCommand>
{
    public RegistrarActividadManoObraCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).NotEmpty();
        RuleFor(x => x.Tipo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.Costo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}
