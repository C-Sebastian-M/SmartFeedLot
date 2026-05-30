using FluentValidation;

namespace Feedlot.Application.Features.Inversion.Commands.CrearEtapaInversion;

public sealed class CrearEtapaInversionCommandValidator : AbstractValidator<CrearEtapaInversionCommand>
{
    public CrearEtapaInversionCommandValidator()
    {
        RuleFor(x => x.Numero).InclusiveBetween(1, 5);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}
