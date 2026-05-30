using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class Empleado : AggregateRoot<Guid>
{
    private readonly List<ActividadManoObra> _actividades = new();

    private Empleado() { }

    private Empleado(Guid id, string nombre, Dinero pagoMensual)
        : base(id)
    {
        Nombre = nombre;
        PagoMensual = pagoMensual;
    }

    public string Nombre { get; private set; } = null!;
    public Dinero PagoMensual { get; private set; } = null!;
    public IReadOnlyCollection<ActividadManoObra> Actividades => _actividades.AsReadOnly();

    public static Empleado Crear(string nombre, Dinero pagoMensual)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del empleado no puede estar vacío.");
        return new Empleado(Guid.NewGuid(), nombre.Trim(), pagoMensual);
    }

    public ActividadManoObra RegistrarActividad(string tipo, DateOnly fecha, Dinero costo)
    {
        var actividad = new ActividadManoObra(Guid.NewGuid(), Id, tipo, fecha, costo);
        _actividades.Add(actividad);
        return actividad;
    }

    public void Modificar(string nombre, Dinero pagoMensual)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del empleado no puede estar vacío.");
        Nombre = nombre.Trim();
        PagoMensual = pagoMensual;
    }
}
