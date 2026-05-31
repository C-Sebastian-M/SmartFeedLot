using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class Marrana : AggregateRoot<Guid>
{
    private readonly List<Camada> _camadas = [];

    private Marrana() { }

    private Marrana(Guid id, string identificacion, DateOnly fechaCompra, Dinero costo)
        : base(id)
    {
        Identificacion = identificacion;
        FechaCompra = fechaCompra;
        Costo = costo;
    }

    public string Identificacion { get; private set; } = null!;
    public DateOnly FechaCompra { get; private set; }
    public Dinero Costo { get; private set; } = null!;
    public IReadOnlyCollection<Camada> Camadas => _camadas.AsReadOnly();

    public static Marrana Crear(string identificacion, DateOnly fechaCompra, Dinero costo)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            throw new DomainException("La identificación de la marrana es requerida.");

        if (costo.Monto < 0)
            throw new DomainException("El costo de la marrana no puede ser negativo.");

        return new Marrana(Guid.NewGuid(), identificacion.Trim(), fechaCompra, costo);
    }

    public Camada RegistrarCamada(DateOnly fechaNacimiento, int nLechones)
    {
        if (fechaNacimiento < FechaCompra)
            throw new DomainException("La fecha de nacimiento no puede ser anterior a la fecha de compra.");

        if (nLechones <= 0)
            throw new DomainException("El número de lechones debe ser mayor a cero.");

        var camada = new Camada(Guid.NewGuid(), Id, fechaNacimiento, nLechones);
        _camadas.Add(camada);
        return camada;
    }
}
