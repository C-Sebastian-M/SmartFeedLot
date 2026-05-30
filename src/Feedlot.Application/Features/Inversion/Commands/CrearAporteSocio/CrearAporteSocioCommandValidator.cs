using FluentValidation;

namespace Feedlot.Application.Features.Inversion.Commands.CrearAporteSocio;

public sealed class CrearAporteSocioCommandValidator : AbstractValidator<CrearAporteSocioCommand>
{
    public CrearAporteSocioCommandValidator()
    {
        RuleFor(x => x.SocioId).NotEmpty();
        RuleFor(x => x.ItemInversionId).NotEmpty();
        RuleFor(x => x.Monto).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}
