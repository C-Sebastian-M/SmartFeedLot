namespace Feedlot.Domain.Common;

/// <summary>
/// Clase base para Aggregate Roots. Extiende Entity y marca el límite
/// de consistencia transaccional. Solo se accede a las entidades internas
/// a través del Aggregate Root.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }

    // Requerido por EF Core.
    protected AggregateRoot() { }
}
