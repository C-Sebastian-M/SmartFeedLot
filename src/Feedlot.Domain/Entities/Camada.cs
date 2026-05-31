using Feedlot.Domain.Enums;

namespace Feedlot.Domain.Entities;

public sealed class Camada
{
    internal Camada(Guid id, Guid marranaId, DateOnly fechaNacimiento, int nLechones)
    {
        Id = id;
        MarranaId = marranaId;
        FechaNacimiento = fechaNacimiento;
        NLechones = nLechones;
        Estado = EstadoCamada.Preceba;
    }

    private Camada() { }

    public Guid Id { get; private set; }
    public Guid MarranaId { get; private set; }
    public DateOnly FechaNacimiento { get; private set; }
    public int NLechones { get; private set; }
    public EstadoCamada Estado { get; private set; }

    public void AvanzarACeba()
    {
        if (Estado != EstadoCamada.Preceba)
            throw new Exceptions.DomainException("Solo las camadas en preceba pueden avanzar a ceba.");
        Estado = EstadoCamada.Ceba;
    }

    public void MarcarVendida()
    {
        if (Estado == EstadoCamada.Vendida)
            throw new Exceptions.DomainException("La camada ya fue vendida.");
        Estado = EstadoCamada.Vendida;
    }
}
