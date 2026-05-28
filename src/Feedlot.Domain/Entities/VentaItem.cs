using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

public sealed class VentaItem : Entity<Guid>
{
    private VentaItem() { }

    private VentaItem(Guid id, Guid animalId, decimal precioVenta, decimal pesoVentaKg)
        : base(id)
    {
        AnimalId = animalId;
        PrecioVenta = precioVenta;
        PesoVentaKg = pesoVentaKg;
    }

    public Guid VentaId { get; private set; }
    public Guid AnimalId { get; private set; }
    public decimal PrecioVenta { get; private set; }
    public decimal PesoVentaKg { get; private set; }

    internal static VentaItem Crear(Guid ventaId, Guid animalId, decimal precioVenta, decimal pesoVentaKg)
    {
        if (precioVenta < 0)
            throw new DomainException("El precio de venta no puede ser negativo.");
        if (pesoVentaKg <= 0)
            throw new DomainException("El peso de venta debe ser mayor a cero.");

        return new VentaItem(Guid.NewGuid(), animalId, precioVenta, pesoVentaKg) { VentaId = ventaId };
    }
}
