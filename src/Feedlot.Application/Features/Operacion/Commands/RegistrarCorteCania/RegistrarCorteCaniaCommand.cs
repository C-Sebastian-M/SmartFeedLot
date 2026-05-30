using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Operacion.Commands.RegistrarCorteCania;

public sealed record RegistrarCorteCaniaCommand(
    Guid CultivoCaniaId, DateOnly Fecha, int NCalles, decimal Horas,
    int BolsasSilo, decimal Melaza, decimal CostoJornal, string Moneda) : ICommand<Guid>;
