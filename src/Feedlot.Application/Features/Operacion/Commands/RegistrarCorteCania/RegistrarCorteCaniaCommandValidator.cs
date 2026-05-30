using FluentValidation;

namespace Feedlot.Application.Features.Operacion.Commands.RegistrarCorteCania;

public sealed class RegistrarCorteCaniaCommandValidator : AbstractValidator<RegistrarCorteCaniaCommand>
{
    public RegistrarCorteCaniaCommandValidator()
    {
        RuleFor(x => x.CultivoCaniaId).NotEmpty();
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.NCalles).GreaterThan(0);
        RuleFor(x => x.Horas).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BolsasSilo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Melaza).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostoJornal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}
