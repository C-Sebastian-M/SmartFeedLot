using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Animals.Commands.ModificarAnimal;

public sealed record ModificarAnimalCommand(
    Guid AnimalId,
    string? Nombre,
    string NumeroArete,
    string Sexo,
    string? Raza,
    DateOnly? FechaNacimiento,
    DateOnly FechaIngreso,
    decimal PesoIngresoKg,
    decimal PrecioCompra,
    string Moneda,
    Guid? NuevoLoteId = null
) : ICommand;
