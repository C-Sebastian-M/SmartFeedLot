using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Ventas.Commands.CrearVenta;

public sealed record VentaAnimalInput(
    Guid AnimalId,
    decimal PrecioVenta,
    decimal PesoVentaKg);

public sealed record CrearVentaCommand(
    Guid CompradorId,
    DateOnly Fecha,
    string Moneda,
    string? Descripcion,
    List<VentaAnimalInput> Animales
) : ICommand<Guid>;
