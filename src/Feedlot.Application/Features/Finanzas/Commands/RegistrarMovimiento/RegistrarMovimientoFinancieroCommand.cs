using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Finanzas.Commands.RegistrarMovimiento;

public sealed record RegistrarMovimientoFinancieroCommand(
    DateOnly Fecha,
    int PeriodoAnio,
    int PeriodoMes,
    Guid CategoriaGastoId,
    decimal Monto,
    string Moneda,
    string Origen,
    string Descripcion,
    Guid? SocioId,
    Guid RegistradoPorId
) : ICommand<Guid>;
