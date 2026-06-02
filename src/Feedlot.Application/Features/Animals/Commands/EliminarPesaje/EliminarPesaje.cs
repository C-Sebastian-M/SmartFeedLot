using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.EliminarPesaje;

public sealed record EliminarPesajeCommand(
    Guid AnimalId,
    Guid PesajeId
) : ICommand;
public sealed class EliminarPesajeCommandValidator
    : AbstractValidator<EliminarPesajeCommand>
{
    public EliminarPesajeCommandValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty().WithMessage("El ID del animal es requerido.");

        RuleFor(x => x.PesajeId)
            .NotEmpty().WithMessage("El ID del pesaje es requerido.");
    }
}

public sealed class EliminarPesajeCommandHandler
    : IRequestHandler<EliminarPesajeCommand, Result>
{
    private readonly IAnimalRepository _animalRepository;

    public EliminarPesajeCommandHandler(IAnimalRepository animalRepository)
    {
        _animalRepository = animalRepository;
    }

    public async Task<Result> Handle(
        EliminarPesajeCommand request,
        CancellationToken ct)
    {
        var animal = await _animalRepository.ObtenerPorIdAsync(request.AnimalId, ct);

        if (animal is null)
            return Result.NotFound(
                $"No se encontró el animal con ID '{request.AnimalId}'.");

        animal.EliminarPesaje(request.PesajeId);

        _animalRepository.Actualizar(animal);

        return Result.Success();
    }
}
