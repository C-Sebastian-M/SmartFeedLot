using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class ActividadManoObra : Entity<Guid>
{
    private ActividadManoObra() { }

    internal ActividadManoObra(Guid id, Guid empleadoId, string tipo, DateOnly fecha, Dinero costo)
        : base(id)
    {
        EmpleadoId = empleadoId;
        Tipo = tipo;
        Fecha = fecha;
        Costo = costo;
    }

    public Guid EmpleadoId { get; private set; }
    public string Tipo { get; private set; } = null!;
    public DateOnly Fecha { get; private set; }
    public Dinero Costo { get; private set; } = null!;

    public void Modificar(string tipo, DateOnly fecha, Dinero costo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new DomainException("El tipo de actividad no puede estar vacío.");
        Tipo = tipo.Trim();
        Fecha = fecha;
        Costo = costo;
    }
}
