using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Operacion.Commands.RetirarAnimalPotrero;

public sealed record RetirarAnimalPotreroCommand(Guid PotreroId, Guid EstanciaId, DateOnly FechaSalida) : ICommand;
