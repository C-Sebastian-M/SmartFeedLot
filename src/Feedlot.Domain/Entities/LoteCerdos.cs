using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class LoteCerdos : AggregateRoot<Guid>
{
    private LoteCerdos() { }

    private LoteCerdos(Guid id, string codigo, DateOnly fechaInicio, int nAnimales,
        decimal pesoPromedioKg, string ciclo, Guid? camadaId, Dinero? precioVentaKg)
        : base(id)
    {
        Codigo = codigo;
        FechaInicio = fechaInicio;
        NAnimales = nAnimales;
        PesoPromedioKg = pesoPromedioKg;
        Ciclo = ciclo;
        CamadaId = camadaId;
        PrecioVentaKg = precioVentaKg;
    }

    public string Codigo { get; private set; } = null!;
    public DateOnly FechaInicio { get; private set; }
    public int NAnimales { get; private set; }
    public decimal PesoPromedioKg { get; private set; }
    public string Ciclo { get; private set; } = null!;
    public Guid? CamadaId { get; private set; }
    public Dinero? PrecioVentaKg { get; private set; }
    public DateOnly? FechaVenta { get; private set; }
    public bool Vendido => FechaVenta.HasValue;

    public static LoteCerdos Crear(string codigo, DateOnly fechaInicio, int nAnimales,
        decimal pesoPromedioKg, string ciclo, Guid? camadaId, Dinero? precioVentaKg)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("El código del lote es requerido.");

        if (nAnimales <= 0)
            throw new DomainException("El número de animales debe ser mayor a cero.");

        if (pesoPromedioKg <= 0)
            throw new DomainException("El peso promedio debe ser mayor a cero.");

        return new LoteCerdos(Guid.NewGuid(), codigo.Trim(), fechaInicio, nAnimales,
            pesoPromedioKg, ciclo, camadaId, precioVentaKg);
    }

    public void RegistrarVenta(DateOnly fechaVenta, Dinero precioVentaKg)
    {
        if (Vendido)
            throw new DomainException("El lote ya fue vendido.");

        FechaVenta = fechaVenta;
        PrecioVentaKg = precioVentaKg;
    }
}
