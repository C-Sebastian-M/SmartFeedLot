using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.Entities;

public sealed class CultivoCania : AggregateRoot<Guid>
{
    private readonly List<CorteCania> _cortes = new();

    private CultivoCania() { }

    private CultivoCania(Guid id, string nombre, int callesTotales)
        : base(id)
    {
        Nombre = nombre;
        CallesTotales = callesTotales;
    }

    public string Nombre { get; private set; } = null!;
    public int CallesTotales { get; private set; }
    public IReadOnlyCollection<CorteCania> Cortes => _cortes.AsReadOnly();

    public int TotalBolsasSilo => _cortes.Sum(c => c.BolsasSilo);
    public decimal TotalHoras => _cortes.Sum(c => c.Horas);

    public static CultivoCania Crear(string nombre, int callesTotales)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del cultivo no puede estar vacío.");
        if (callesTotales <= 0)
            throw new DomainException("El número de calles totales debe ser mayor a cero.");
        return new CultivoCania(Guid.NewGuid(), nombre.Trim(), callesTotales);
    }

    public CorteCania RegistrarCorte(DateOnly fecha, int nCalles, decimal horas,
        int bolsasSilo, decimal melaza, decimal costoJornal, string moneda)
    {
        if (nCalles <= 0)
            throw new DomainException("El número de calles cortadas debe ser mayor a cero.");

        var corte = new CorteCania(Guid.NewGuid(), Id, fecha, nCalles, horas,
            bolsasSilo, melaza, costoJornal, moneda);
        _cortes.Add(corte);
        return corte;
    }

    public void Modificar(string nombre, int callesTotales)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del cultivo no puede estar vacío.");
        if (callesTotales <= 0)
            throw new DomainException("El número de calles totales debe ser mayor a cero.");
        Nombre = nombre.Trim();
        CallesTotales = callesTotales;
    }
}
