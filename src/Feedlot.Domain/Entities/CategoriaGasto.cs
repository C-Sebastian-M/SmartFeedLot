using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

public sealed class CategoriaGasto : AggregateRoot<Guid>
{
    private CategoriaGasto() { }

    private CategoriaGasto(Guid id, string nombre, TipoCategoriaGasto tipo)
        : base(id)
    {
        Nombre = nombre;
        Tipo = tipo;
    }

    public string Nombre { get; private set; } = null!;
    public TipoCategoriaGasto Tipo { get; private set; }

    public static CategoriaGasto Crear(string nombre, TipoCategoriaGasto tipo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la categoría no puede estar vacío.");

        return new CategoriaGasto(Guid.NewGuid(), nombre.Trim(), tipo);
    }

    public void Modificar(string nombre, TipoCategoriaGasto tipo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la categoría no puede estar vacío.");

        Nombre = nombre.Trim();
        Tipo = tipo;
    }
}
