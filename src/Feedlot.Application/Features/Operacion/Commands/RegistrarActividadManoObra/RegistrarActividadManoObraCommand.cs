using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Operacion.Commands.RegistrarActividadManoObra;

public sealed record RegistrarActividadManoObraCommand(
    Guid EmpleadoId, string Tipo, DateOnly Fecha, decimal Costo, string Moneda) : ICommand<Guid>;
