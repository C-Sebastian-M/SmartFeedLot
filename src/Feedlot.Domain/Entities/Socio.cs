using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

public sealed class Socio : AggregateRoot<Guid>
{
    private Socio() { }

    private Socio(Guid id, string nombre, decimal participacion)
        : base(id)
    {
        Nombre = nombre;
        Participacion = participacion;
    }

    public string Nombre { get; private set; } = null!;
    
    /// <summary>
    /// Porcentaje de participación (por ejemplo, 50.00 para 50%).
    /// </summary>
    public decimal Participacion { get; private set; }

    public static Socio Crear(string nombre, decimal participacion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del socio no puede estar vacío.");

        if (participacion < 0 || participacion > 100)
            throw new DomainException("La participación debe estar entre 0% y 100%.");

        return new Socio(Guid.NewGuid(), nombre.Trim(), participacion);
    }

    public void Modificar(string nombre, decimal participacion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del socio no puede estar vacío.");

        if (participacion < 0 || participacion > 100)
            throw new DomainException("La participación debe estar entre 0% y 100%.");

        Nombre = nombre.Trim();
        Participacion = participacion;
    }
}
