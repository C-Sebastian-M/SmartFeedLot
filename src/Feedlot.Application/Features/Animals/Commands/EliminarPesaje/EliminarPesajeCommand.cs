using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Animals.Commands.EliminarPesaje;

public sealed record EliminarPesajeCommand(
    Guid AnimalId,
    Guid PesajeId
) : ICommand;
