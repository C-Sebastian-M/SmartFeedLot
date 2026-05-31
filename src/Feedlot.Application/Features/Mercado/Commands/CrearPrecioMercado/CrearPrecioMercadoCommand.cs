using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Mercado.Commands.CrearPrecioMercado;

public sealed record CrearPrecioMercadoCommand(
    DateOnly Fecha,
    string Especie,
    string Tipo,
    decimal PrecioPorKg,
    string Fuente
) : ICommand<Guid>;
