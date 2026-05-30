using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Operacion.Commands.CrearEmpleado;

public sealed record CrearEmpleadoCommand(string Nombre, decimal PagoMensual, string Moneda) : ICommand<Guid>;
