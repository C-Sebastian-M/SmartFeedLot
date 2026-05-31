using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Porcino.Commands.RegistrarCamada;

public sealed record RegistrarCamadaCommand(
    Guid MarranaId,
    DateOnly FechaNacimiento,
    int NLechones) : ICommand<Guid>;
