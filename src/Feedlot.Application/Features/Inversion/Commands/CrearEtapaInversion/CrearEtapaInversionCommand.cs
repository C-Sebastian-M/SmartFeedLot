using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Inversion.Commands.CrearEtapaInversion;

public sealed record CrearEtapaInversionCommand(
    int Numero,
    string Nombre
) : ICommand<Guid>;
