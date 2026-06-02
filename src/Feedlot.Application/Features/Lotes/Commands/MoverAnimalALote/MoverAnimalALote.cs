using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Services;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Commands.MoverAnimalALote;

public sealed record MoverAnimalALoteCommand(
    Guid AnimalId,
    Guid LoteDestinoId,
    DateOnly FechaMovimiento,
    string Motivo
) : ICommand;
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

public sealed class MoverAnimalALoteCommandHandler
    : IRequestHandler<MoverAnimalALoteCommand, Result>
{
    private readonly AnimalLoteService _animalLoteService;

    public MoverAnimalALoteCommandHandler(AnimalLoteService animalLoteService)
    {
        _animalLoteService = animalLoteService;
    }

    public async Task<Result> Handle(MoverAnimalALoteCommand request, CancellationToken ct)
    {
        var motivo = Enum.Parse<MotivoMovimiento>(request.Motivo, ignoreCase: true);

        // El Domain Service valida la invariante de pertenencia única
        // y coordina el retiro del lote origen y el ingreso al destino.
        await _animalLoteService.MoverAnimalAsync(
            request.AnimalId,
            request.LoteDestinoId,
            request.FechaMovimiento,
            motivo,
            ct);

        return Result.Success();
    }
}
