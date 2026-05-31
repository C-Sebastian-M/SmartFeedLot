using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class Venta : AggregateRoot<Guid>
{
    private readonly List<VentaItem> _items = [];

    private Venta() { }

    private Venta(Guid id, Guid compradorId, DateOnly fecha, string moneda, string? descripcion,
        CanalVenta canal, decimal? comisionPct, Dinero? costoTransporte)
        : base(id)
    {
        CompradorId = compradorId;
        Fecha = fecha;
        Moneda = moneda;
        Descripcion = descripcion;
        Canal = canal;
        ComisionPct = comisionPct;
        CostoTransporte = costoTransporte;
    }

    public Guid CompradorId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal MontoTotal => _items.Sum(i => i.PrecioVenta);
    public string Moneda { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public CanalVenta Canal { get; private set; }
    public decimal? ComisionPct { get; private set; }
    public Dinero? CostoTransporte { get; private set; }
    public IReadOnlyCollection<VentaItem> Items => _items.AsReadOnly();

    public static Venta Crear(Guid compradorId, DateOnly fecha, string moneda, string? descripcion,
        CanalVenta canal = CanalVenta.Directa, decimal? comisionPct = null, Dinero? costoTransporte = null)
    {
        if (string.IsNullOrWhiteSpace(moneda))
            throw new DomainException("La moneda es requerida.");

        return new Venta(Guid.NewGuid(), compradorId, fecha, moneda.Trim(), descripcion?.Trim(),
            canal, comisionPct, costoTransporte);
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
