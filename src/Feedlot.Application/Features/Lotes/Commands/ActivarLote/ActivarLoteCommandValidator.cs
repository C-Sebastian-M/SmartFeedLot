using FluentValidation;

namespace Feedlot.Application.Features.Lotes.Commands.ActivarLote;

public sealed class ActivarLoteCommandValidator : AbstractValidator<ActivarLoteCommand>
{
    public ActivarLoteCommandValidator()
    {
        RuleFor(x => x.LoteId)
            .NotEmpty().WithMessage("El ID del lote es requerido.");
    }
}
