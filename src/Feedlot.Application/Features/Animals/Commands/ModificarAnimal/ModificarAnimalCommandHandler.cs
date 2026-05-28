using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.ModificarAnimal;

public sealed class ModificarAnimalCommandHandler
    : IRequestHandler<ModificarAnimalCommand, Result>
{
    private readonly IAnimalRepository _animalRepository;

    public ModificarAnimalCommandHandler(IAnimalRepository animalRepository)
    {
        _animalRepository = animalRepository;
    }

    public async Task<Result> Handle(
        ModificarAnimalCommand request,
        CancellationToken ct)
    {
        var animal = await _animalRepository.ObtenerPorIdAsync(request.AnimalId, ct);

        if (animal is null)
            return Result.NotFound(
                $"No se encontró el animal con ID '{request.AnimalId}'.");

        var sexo = Enum.Parse<Sexo>(request.Sexo, ignoreCase: true);
        var pesoIngreso = Peso.Crear(request.PesoIngresoKg);
        var precioCompra = Dinero.Crear(request.PrecioCompra, request.Moneda);

        animal.Modificar(
            request.Nombre,
            request.NumeroArete,
            request.Raza,
            request.FechaNacimiento,
            pesoIngreso,
            precioCompra);

        _animalRepository.Actualizar(animal);

        return Result.Success();
    }
}
