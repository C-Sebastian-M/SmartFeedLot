using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearCategoriaGasto;

public sealed record CrearCategoriaGastoCommand(
    string Nombre,
    string Tipo
) : ICommand<Guid>;
