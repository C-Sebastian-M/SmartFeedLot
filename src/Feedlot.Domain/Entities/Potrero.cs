using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

public sealed class Potrero : AggregateRoot<Guid>
{
    private readonly List<EstanciaAnimal> _estancias = new();

    private Potrero() { }

    private Potrero(Guid id, string nombre, int capacidad)
        : base(id)
    {
        Nombre = nombre;
        Capacidad = capacidad;
    }

    public string Nombre { get; private set; } = null!;
    public int Capacidad { get; private set; }
    public IReadOnlyCollection<EstanciaAnimal> Estancias => _estancias.AsReadOnly();

    public int AnimalesActuales => _estancias.Count(e => e.Salida == null);

    public static Potrero Crear(string nombre, int capacidad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del potrero no puede estar vacío.");
        if (capacidad <= 0)
            throw new DomainException("La capacidad del potrero debe ser mayor a cero.");
        return new Potrero(Guid.NewGuid(), nombre.Trim(), capacidad);
    }

    public EstanciaAnimal IngresarAnimal(Guid animalId, DateOnly fechaEntrada)
    {
        if (AnimalesActuales >= Capacidad)
            throw new DomainException($"El potrero '{Nombre}' está lleno (capacidad: {Capacidad}).");

        if (_estancias.Any(e => e.AnimalId == animalId && e.Salida == null))
            throw new DomainException($"El animal ya se encuentra en el potrero '{Nombre}'.");

        var estancia = new EstanciaAnimal(Guid.NewGuid(), Id, animalId, fechaEntrada);
        _estancias.Add(estancia);
        return estancia;
    }

    public void RetirarAnimal(Guid estanciaId, DateOnly fechaSalida)
    {
        var estancia = _estancias.FirstOrDefault(e => e.Id == estanciaId && e.Salida == null);
        if (estancia is null)
            throw new DomainException("La estancia no existe o el animal ya fue retirado.");
        estancia.RegistrarSalida(fechaSalida);
    }

    public void Modificar(string nombre, int capacidad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del potrero no puede estar vacío.");
        if (capacidad <= 0)
            throw new DomainException("La capacidad debe ser mayor a cero.");
        Nombre = nombre.Trim();
        Capacidad = capacidad;
    }
}
