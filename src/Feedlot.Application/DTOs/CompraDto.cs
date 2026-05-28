namespace Feedlot.Application.DTOs;

public sealed class CompraDto
{
    public Guid Id { get; set; }
    public Guid ProveedorId { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string TipoCompra { get; set; } = string.Empty;
    public decimal CostoTotal { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? CantidadCabezas { get; set; }
    public decimal? PrecioPorCabeza { get; set; }
    public decimal? PesoPromedioKg { get; set; }
    public Guid? LoteId { get; set; }
    public string? TipoInsumo { get; set; }
    public decimal? CantidadInsumo { get; set; }
    public string? UnidadMedida { get; set; }
}
