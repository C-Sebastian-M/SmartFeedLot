using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Operacion.Commands.CrearCultivoCania;

public sealed record CrearCultivoCaniaCommand(string Nombre, int CallesTotales) : ICommand<Guid>;
