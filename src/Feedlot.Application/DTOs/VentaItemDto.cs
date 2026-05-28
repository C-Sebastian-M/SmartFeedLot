namespace Feedlot.Application.DTOs;

public sealed class VentaItemDto
{
    public Guid Id { get; set; }
    public Guid VentaId { get; set; }
    public Guid AnimalId { get; set; }
    public string CodigoAnimal { get; set; } = string.Empty;
    public string? NombreAnimal { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal PesoVentaKg { get; set; }
}
