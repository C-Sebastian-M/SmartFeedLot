using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarPesaje;

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
