namespace Feedlot.Application.Features.Mercado.Queries.ObtenerPreciosMercado;

public sealed record PrecioMercadoDto(
    Guid Id,
    DateOnly Fecha,
    string Especie,
    string Tipo,
    decimal PrecioPorKg,
    string Fuente);
