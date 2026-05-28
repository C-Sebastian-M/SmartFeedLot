using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Animals.Commands.EliminarAnimal;

public sealed record EliminarAnimalCommand(Guid AnimalId) : ICommand;
