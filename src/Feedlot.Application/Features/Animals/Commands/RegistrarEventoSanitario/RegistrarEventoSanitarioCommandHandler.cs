using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarEventoSanitario;

public sealed class RegistrarEventoSanitarioCommandHandler
    : IRequestHandler<RegistrarEventoSanitarioCommand, Result<Guid>>
{
    private readonly IAnimalRepository _animalRepository;

    public RegistrarEventoSanitarioCommandHandler(IAnimalRepository animalRepository)
    {
        _animalRepository = animalRepository;
    }

    public async Task<Result<Guid>> Handle(
        RegistrarEventoSanitarioCommand request,
        CancellationToken ct)
    {
        var animal = await _animalRepository.ObtenerPorIdAsync(request.AnimalId, ct);

        if (animal is null)
            return Result<Guid>.NotFound(
                $"No se encontró el animal con ID '{request.AnimalId}'.");

        var severidad = Enum.Parse<SeveridadEvento>(request.Severidad, ignoreCase: true);

        var evento = animal.RegistrarEventoSanitario(
            request.FechaEvento,
            request.Diagnostico,
            request.Descripcion,
            severidad,
            request.Tratamiento,
            request.TipoEvento,
            request.ProximaDosis,
            request.Responsable);

        _animalRepository.Actualizar(animal);

        return Result<Guid>.Success(evento.Id);
    }
}
