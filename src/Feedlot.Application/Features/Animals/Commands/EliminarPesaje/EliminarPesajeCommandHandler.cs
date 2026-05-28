using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.EliminarPesaje;

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
