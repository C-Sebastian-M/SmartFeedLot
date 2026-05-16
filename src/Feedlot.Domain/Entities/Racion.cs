using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Aggregate Root: Racion.
/// Representa una fórmula de alimentación con sus ingredientes y valores nutricionales.
/// Es referenciada por ConsumoAlimenticio pero vive en el bounded context de Nutrición.
/// Aquí se modela como un aggregate del mismo proyecto para simplificar la fase 1.
/// </summary>
public sealed class Racion : AggregateRoot<Guid>
{
    private readonly List<RacionIngrediente> _ingredientes = [];

    private Racion() { } // EF Core

    private Racion(
        Guid id,
        string nombre,
        Dinero costoKg,
        decimal proteinaPct,
        decimal energiaMcal) : base(id)
    {
        Nombre = nombre;
        CostoKg = costoKg;
        ProteinaPct = proteinaPct;
        EnergiaMcal = energiaMcal;
        Activa = true;
    }

    public string Nombre { get; private set; } = null!;
    public Dinero CostoKg { get; private set; } = null!;
    public decimal ProteinaPct { get; private set; }
    public decimal EnergiaMcal { get; private set; }
    public bool Activa { get; private set; }

    public IReadOnlyCollection<RacionIngrediente> Ingredientes => _ingredientes.AsReadOnly();

    public static Racion Crear(
        string nombre,
        Dinero costoKg,
        decimal proteinaPct,
        decimal energiaMcal)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la ración no puede estar vacío.");

        if (proteinaPct is < 0 or > 100)
            throw new DomainException(
                $"El porcentaje de proteína debe estar entre 0 y 100. Recibido: {proteinaPct}.");

        if (energiaMcal < 0)
            throw new DomainException(
                $"La energía no puede ser negativa. Recibido: {energiaMcal} Mcal.");

        return new Racion(Guid.NewGuid(), nombre.Trim(), costoKg, proteinaPct, energiaMcal);
    }

    public void AgregarIngrediente(Guid ingredienteId, decimal proporcionPct)
    {
        var totalActual = _ingredientes.Sum(i => i.ProporcionPct);
        if (totalActual + proporcionPct > 100)
            throw new DomainException(
                $"La suma de proporciones de ingredientes no puede superar 100%. " +
                $"Actual: {totalActual}%, intento agregar: {proporcionPct}%.");

        var yaExiste = _ingredientes.Any(i => i.IngredienteId == ingredienteId);
        if (yaExiste)
            throw new DomainException(
                $"El ingrediente '{ingredienteId}' ya está incluido en esta ración.");

        _ingredientes.Add(RacionIngrediente.Crear(Id, ingredienteId, proporcionPct));
    }

    public void Desactivar() => Activa = false;
}
