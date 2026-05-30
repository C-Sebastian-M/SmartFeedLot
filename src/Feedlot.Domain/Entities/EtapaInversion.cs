using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class EtapaInversion : AggregateRoot<Guid>
{
    private readonly List<ItemInversion> _items = new();

    private EtapaInversion() { }

    private EtapaInversion(Guid id, int numero, string nombre)
        : base(id)
    {
        Numero = numero;
        Nombre = nombre;
    }

    public int Numero { get; private set; }
    public string Nombre { get; private set; } = null!;

    public IReadOnlyCollection<ItemInversion> Items => _items.AsReadOnly();

    public decimal TotalRealizadoMonto => Items
        .Where(i => i.Estado == EstadoItemInversion.OK)
        .Sum(i => i.Costo.Monto);

    public decimal TotalPendienteMonto => Items
        .Where(i => i.Estado == EstadoItemInversion.Pendiente)
        .Sum(i => i.Costo.Monto);

    public string Moneda => Items.FirstOrDefault()?.Costo.Moneda ?? "COP";

    public static EtapaInversion Crear(int numero, string nombre)
    {
        if (numero < 1 || numero > 5)
            throw new DomainException("El número de etapa debe estar entre 1 y 5.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la etapa no puede estar vacío.");

        return new EtapaInversion(Guid.NewGuid(), numero, nombre.Trim());
    }

    public ItemInversion AgregarItem(string producto, Dinero costo,
        string? observacion, EstadoItemInversion estado, decimal porcentajeAvance)
    {
        var item = new ItemInversion(
            Guid.NewGuid(), Id, producto, costo, observacion, estado,
            Math.Clamp(porcentajeAvance, 0, 100));

        _items.Add(item);
        return item;
    }

    public void Modificar(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la etapa no puede estar vacío.");
        Nombre = nombre.Trim();
    }
}
