using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

/// <summary>Entity interna de Racion. Representa un ingrediente con su proporción.</summary>
public sealed class RacionIngrediente : Entity<Guid>
{
    private RacionIngrediente() { } // EF Core

    private RacionIngrediente(Guid id, Guid racionId, Guid ingredienteId, decimal proporcionPct)
        : base(id)
    {
        RacionId = racionId;
        IngredienteId = ingredienteId;
        ProporcionPct = proporcionPct;
    }

    public Guid RacionId { get; private set; }
    public Guid IngredienteId { get; private set; }
    public decimal ProporcionPct { get; private set; }

    internal static RacionIngrediente Crear(Guid racionId, Guid ingredienteId, decimal proporcionPct)
    {
        if (proporcionPct <= 0 || proporcionPct > 100)
            throw new DomainException(
                $"La proporción del ingrediente debe estar entre 0 y 100%. Recibido: {proporcionPct}%.");

        return new RacionIngrediente(Guid.NewGuid(), racionId, ingredienteId, proporcionPct);
    }
}
