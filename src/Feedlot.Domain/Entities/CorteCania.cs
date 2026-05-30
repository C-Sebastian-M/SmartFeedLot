using Feedlot.Domain.Common;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class CorteCania : Entity<Guid>
{
    private CorteCania() { }

    internal CorteCania(Guid id, Guid cultivoCaniaId, DateOnly fecha, int nCalles,
        decimal horas, int bolsasSilo, decimal melaza, decimal costoJornal, string moneda)
        : base(id)
    {
        CultivoCaniaId = cultivoCaniaId;
        Fecha = fecha;
        NCalles = nCalles;
        Horas = horas;
        BolsasSilo = bolsasSilo;
        Melaza = melaza;
        CostoJornal = Dinero.Crear(costoJornal, moneda);
    }

    public Guid CultivoCaniaId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public int NCalles { get; private set; }
    public decimal Horas { get; private set; }
    public int BolsasSilo { get; private set; }
    public decimal Melaza { get; private set; }
    public Dinero CostoJornal { get; private set; } = null!;
}
