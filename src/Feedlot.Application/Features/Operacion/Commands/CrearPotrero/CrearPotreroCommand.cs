using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Operacion.Commands.CrearPotrero;

public sealed record CrearPotreroCommand(string Nombre, int Capacidad) : ICommand<Guid>;
