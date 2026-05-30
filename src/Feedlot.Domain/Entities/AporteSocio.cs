using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class AporteSocio : Entity<Guid>
{
    private AporteSocio() { }

    private AporteSocio(Guid id, Guid socioId, Guid itemInversionId, Dinero monto)
        : base(id)
    {
        SocioId = socioId;
        ItemInversionId = itemInversionId;
        Monto = monto;
    }

    public Guid SocioId { get; private set; }
    public Guid ItemInversionId { get; private set; }
    public Dinero Monto { get; private set; } = null!;

    public static AporteSocio Crear(Guid socioId, Guid itemInversionId, Dinero monto)
    {
        if (monto.Monto < 0)
            throw new DomainException("El monto del aporte no puede ser negativo.");

        return new AporteSocio(Guid.NewGuid(), socioId, itemInversionId, monto);
    }
}
