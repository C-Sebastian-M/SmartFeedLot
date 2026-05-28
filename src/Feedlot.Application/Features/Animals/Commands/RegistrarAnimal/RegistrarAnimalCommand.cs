using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarAnimal;

public sealed record RegistrarAnimalCommand(
    string? Nombre,
    string Sexo,
    string? Raza,
    DateOnly? FechaNacimiento,
    decimal PesoIngresoKg,
    decimal PrecioCompraPorKg,
    string Moneda,
    DateOnly FechaIngreso,
    Guid? LoteInicialId
) : ICommand<Guid>;
