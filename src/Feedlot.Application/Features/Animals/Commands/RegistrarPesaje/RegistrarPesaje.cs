using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarPesaje;

public sealed record RegistrarPesajeCommand(
    Guid AnimalId,
    DateOnly FechaPesaje,
    decimal PesoKg,
    string? Observaciones
) : ICommand<Guid>;
public sealed class RegistrarPesajeCommandValidator
    : AbstractValidator<RegistrarPesajeCommand>
{
    public RegistrarPesajeCommandValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty().WithMessage("El ID del animal es requerido.");

        RuleFor(x => x.FechaPesaje)
            .NotEmpty().WithMessage("La fecha del pesaje es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha del pesaje no puede ser futura.");

        RuleFor(x => x.PesoKg)
            .GreaterThan(0).WithMessage("El peso debe ser mayor a cero.")
            .LessThan(2000).WithMessage("El peso parece inválido (máx 2000 kg).");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden superar 500 caracteres.")
            .When(x => x.Observaciones is not null);
    }
}

public sealed class RegistrarPesajeCommandHandler
    : IRequestHandler<RegistrarPesajeCommand, Result<Guid>>
{
    private readonly IAnimalRepository _animalRepository;

    public RegistrarPesajeCommandHandler(IAnimalRepository animalRepository)
    {
        _animalRepository = animalRepository;
    }

    public async Task<Result<Guid>> Handle(
        RegistrarPesajeCommand request,
        CancellationToken ct)
    {
        var animal = await _animalRepository.ObtenerPorIdAsync(request.AnimalId, ct);

        if (animal is null)
            return Result<Guid>.NotFound(
                $"No se encontró el animal con ID '{request.AnimalId}'.");

        var peso = Peso.Crear(request.PesoKg);

        // El Aggregate valida estado activo y orden cronológico.
        // Si falla una invariante, lanza DomainException → ExceptionHandlingMiddleware → HTTP 422.
        var pesaje = animal.RegistrarPesaje(request.FechaPesaje, peso, request.Observaciones);

        _animalRepository.Actualizar(animal);

        return Result<Guid>.Success(pesaje.Id);
    }
}
