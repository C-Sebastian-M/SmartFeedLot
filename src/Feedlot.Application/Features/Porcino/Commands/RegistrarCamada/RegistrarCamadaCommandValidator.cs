using FluentValidation;

namespace Feedlot.Application.Features.Porcino.Commands.RegistrarCamada;

public sealed class RegistrarCamadaCommandValidator : AbstractValidator<RegistrarCamadaCommand>
{
    public RegistrarCamadaCommandValidator()
    {
        RuleFor(x => x.MarranaId).NotEmpty();
        RuleFor(x => x.FechaNacimiento).NotEmpty();
        RuleFor(x => x.NLechones).GreaterThan(0);
    }
}
