using Feedlot.Domain.Common;

namespace Feedlot.Domain.Entities;

public sealed class Proveedor : Entity<Guid>
{
    private Proveedor() { }

    private Proveedor(Guid id, string nombre, string? contacto, string? telefono, string? email)
        : base(id)
    {
        Nombre = nombre;
        Contacto = contacto;
        Telefono = telefono;
        Email = email;
    }

    public string Nombre { get; private set; } = null!;
    public string? Contacto { get; private set; }
    public string? Telefono { get; private set; }
    public string? Email { get; private set; }

    public static Proveedor Crear(string nombre, string? contacto, string? telefono, string? email)
    {
        return new Proveedor(Guid.NewGuid(), nombre.Trim(), contacto?.Trim(), telefono?.Trim(), email?.Trim());
    }

    public void Modificar(string nombre, string? contacto, string? telefono, string? email)
    {
        Nombre = nombre.Trim();
        Contacto = contacto?.Trim();
        Telefono = telefono?.Trim();
        Email = email?.Trim();
    }
}