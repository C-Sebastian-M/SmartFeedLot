using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearPrestamo;

public sealed record CrearPrestamoCommand(
    decimal Monto,
    string Moneda,
    decimal TasaMensual,
    int NCuotas,
    DateOnly FechaInicio,
    string Descripcion
) : ICommand<Guid>;
