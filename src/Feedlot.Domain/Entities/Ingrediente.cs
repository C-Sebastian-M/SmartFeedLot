using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Aggregate Root: Ingrediente.
/// Representa un insumo alimenticio con su perfil nutricional y costo.
/// </summary>
public sealed class Ingrediente : AggregateRoot<Guid>
{
    private Ingrediente() { } // EF Core

    private Ingrediente(
        Guid id,
        string nombre,
        Dinero costoKg,
        decimal proteinaPct,
        string unidadMedida) : base(id)
    {
        Nombre = nombre;
        CostoKg = costoKg;
        ProteinaPct = proteinaPct;
        UnidadMedida = unidadMedida;
    }

    public string Nombre { get; private set; } = null!;
    public Dinero CostoKg { get; private set; } = null!;
    public decimal ProteinaPct { get; private set; }
    public string UnidadMedida { get; private set; } = null!;

    public static Ingrediente Crear(
        string nombre,
        Dinero costoKg,
        decimal proteinaPct,
        string unidadMedida = "kg")
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del ingrediente no puede estar vacío.");

        if (proteinaPct is < 0 or > 100)
            throw new DomainException(
                $"El porcentaje de proteína debe estar entre 0 y 100. Recibido: {proteinaPct}.");

        return new Ingrediente(Guid.NewGuid(), nombre.Trim(), costoKg, proteinaPct, unidadMedida.Trim());
    }

    public void ActualizarCosto(Dinero nuevoCosto)
        => CostoKg = nuevoCosto;
}
