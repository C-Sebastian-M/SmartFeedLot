using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Operacion.Commands.IngresarAnimalPotrero;

public sealed record IngresarAnimalPotreroCommand(Guid PotreroId, Guid AnimalId, DateOnly FechaEntrada) : ICommand<Guid>;
