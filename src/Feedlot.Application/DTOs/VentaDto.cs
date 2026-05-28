namespace Feedlot.Application.DTOs;

public sealed class VentaDto
{
    public Guid Id { get; set; }
    public Guid CompradorId { get; set; }
    public string NombreComprador { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public decimal MontoTotal { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int TotalAnimales { get; set; }
    public List<VentaItemDto> Items { get; set; } = [];
}
