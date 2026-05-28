using Feedlot.Domain.Common;

namespace Feedlot.Domain.Entities;

public sealed class Compra : Entity<Guid>
{
    private Compra() { }

    private Compra(
        Guid id,
        Guid proveedorId,
        DateOnly fecha,
        string tipoCompra,
        decimal costoTotal,
        string moneda,
        string? descripcion,
        int? cantidadCabezas,
        decimal? precioPorCabeza,
        decimal? pesoPromedioKg,
        Guid? loteId,
        string? tipoInsumo,
        decimal? cantidadInsumo,
        string? unidadMedida)
        : base(id)
    {
        ProveedorId = proveedorId;
        Fecha = fecha;
        TipoCompra = tipoCompra;
        CostoTotal = costoTotal;
        Moneda = moneda;
        Descripcion = descripcion;
        CantidadCabezas = cantidadCabezas;
        PrecioPorCabeza = precioPorCabeza;
        PesoPromedioKg = pesoPromedioKg;
        LoteId = loteId;
        TipoInsumo = tipoInsumo;
        CantidadInsumo = cantidadInsumo;
        UnidadMedida = unidadMedida;
    }

    public Guid ProveedorId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string TipoCompra { get; private set; } = null!; // "Ganado" | "Insumo"
    public decimal CostoTotal { get; private set; }
    public string Moneda { get; private set; } = null!;
    public string? Descripcion { get; private set; }

    // Campos para compra de ganado
    public int? CantidadCabezas { get; private set; }
    public decimal? PrecioPorCabeza { get; private set; }
    public decimal? PesoPromedioKg { get; private set; }
    public Guid? LoteId { get; private set; }

    // Campos para compra de insumos
    public string? TipoInsumo { get; private set; }
    public decimal? CantidadInsumo { get; private set; }
    public string? UnidadMedida { get; private set; }

    public static Compra CrearCompraGanado(
        Guid proveedorId,
        DateOnly fecha,
        int cantidadCabezas,
        decimal precioPorCabeza,
        decimal pesoPromedioKg,
        Guid loteId,
        decimal costoTotal,
        string moneda,
        string? descripcion)
    {
        return new Compra(
            Guid.NewGuid(), proveedorId, fecha, "Ganado",
            costoTotal, moneda, descripcion,
            cantidadCabezas, precioPorCabeza, pesoPromedioKg, loteId,
            null, null, null);
    }

    public static Compra CrearCompraInsumo(
        Guid proveedorId,
        DateOnly fecha,
        string tipoInsumo,
        decimal cantidad,
        string unidadMedida,
        decimal costoTotal,
        string moneda,
        string? descripcion)
    {
        return new Compra(
            Guid.NewGuid(), proveedorId, fecha, "Insumo",
            costoTotal, moneda, descripcion,
            null, null, null, null,
            tipoInsumo, cantidad, unidadMedida);
    }
}