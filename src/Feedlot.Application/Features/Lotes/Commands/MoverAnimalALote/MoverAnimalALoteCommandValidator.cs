using FluentValidation;
using Feedlot.Domain.Enums;

namespace Feedlot.Application.Features.Lotes.Commands.MoverAnimalALote;

public sealed class MoverAnimalALoteCommandValidator
    : AbstractValidator<MoverAnimalALoteCommand>
{
    public MoverAnimalALoteCommandValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty().WithMessage("El ID del animal es requerido.");

        RuleFor(x => x.LoteDestinoId)
            .NotEmpty().WithMessage("El ID del lote destino es requerido.")
            .NotEqual(x => x.AnimalId)
            .WithMessage("El lote destino no puede ser el mismo que el animal.");

        RuleFor(x => x.FechaMovimiento)
            .NotEmpty().WithMessage("La fecha del movimiento es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha del movimiento no puede ser futura.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo del movimiento es requerido.")
            .Must(m => Enum.TryParse<MotivoMovimiento>(m, ignoreCase: true, out _))
            .WithMessage("Motivo inválido. Valores: IngresoInicial, Reclasificacion, Sanitario, Capacidad, Venta, Muerte, Otro.");
    }
}
