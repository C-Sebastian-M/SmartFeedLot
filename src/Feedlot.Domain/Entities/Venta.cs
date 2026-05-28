using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

public sealed class Venta : AggregateRoot<Guid>
{
    private readonly List<VentaItem> _items = [];

    private Venta() { }

    private Venta(Guid id, Guid compradorId, DateOnly fecha, string moneda, string? descripcion)
        : base(id)
    {
        CompradorId = compradorId;
        Fecha = fecha;
        Moneda = moneda;
        Descripcion = descripcion;
    }

    public Guid CompradorId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal MontoTotal => _items.Sum(i => i.PrecioVenta);
    public string Moneda { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public IReadOnlyCollection<VentaItem> Items => _items.AsReadOnly();

    public static Venta Crear(Guid compradorId, DateOnly fecha, string moneda, string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(moneda))
            throw new DomainException("La moneda es requerida.");

        return new Venta(Guid.NewGuid(), compradorId, fecha, moneda.Trim(), descripcion?.Trim());
    }

    public VentaItem AgregarItem(Guid animalId, decimal precioVenta, decimal pesoVentaKg)
    {
        if (_items.Any(i => i.AnimalId == animalId))
            throw new DomainException($"El animal '{animalId}' ya está en esta venta.");

        var item = VentaItem.Crear(Id, animalId, precioVenta, pesoVentaKg);
        _items.Add(item);
        return item;
    }
}
