using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.EliminarAnimal;

public sealed class EliminarAnimalCommandHandler
    : IRequestHandler<EliminarAnimalCommand, Result>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly ILoteRepository _loteRepository;

    public EliminarAnimalCommandHandler(
        IAnimalRepository animalRepository,
        ILoteRepository loteRepository)
    {
        _animalRepository = animalRepository;
        _loteRepository = loteRepository;
    }

    public async Task<Result> Handle(
        EliminarAnimalCommand request,
        CancellationToken ct)
    {
        var animal = await _animalRepository.ObtenerPorIdAsync(request.AnimalId, ct);

        if (animal is null)
            return Result.NotFound(
                $"No se encontró el animal con ID '{request.AnimalId}'.");

        // Retirar el animal del lote activo si está en uno
        var loteActivo = await _loteRepository.ObtenerLoteActivoDelAnimalAsync(request.AnimalId, ct);
        if (loteActivo is not null)
        {
            loteActivo.RetirarAnimal(request.AnimalId,
                DateOnly.FromDateTime(DateTime.UtcNow),
                Domain.Enums.MotivoMovimiento.Otro);
            _loteRepository.Actualizar(loteActivo);
        }

        await _animalRepository.EliminarAsync(request.AnimalId, ct);

        return Result.Success();
    }
}
