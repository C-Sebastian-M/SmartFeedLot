namespace Feedlot.Application.DTOs;

public sealed class PesajeDto
{
    public Guid Id { get; init; }
    public Guid AnimalId { get; init; }
    public DateOnly FechaPesaje { get; init; }
    public decimal PesoKg { get; init; }
    public string? Observaciones { get; init; }
}

public sealed class EventoSanitarioDto
{
    public Guid Id { get; init; }
    public Guid AnimalId { get; init; }
    public DateOnly FechaEvento { get; init; }
    public string Diagnostico { get; init; } = null!;
    public string Descripcion { get; init; } = null!;
    public string Severidad { get; init; } = null!;
    public string? Tratamiento { get; init; }
}

public sealed class ConsumoAlimenticioDto
{
    public Guid Id { get; init; }
    public Guid LoteId { get; init; }
    public Guid RacionId { get; init; }
    public string NombreRacion { get; init; } = null!;
    public DateOnly Fecha { get; init; }
    public decimal CantidadKg { get; init; }
    public decimal CostoTotal { get; init; }
    public string Moneda { get; init; } = null!;
}

public sealed class RacionDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = null!;
    public decimal CostoKg { get; init; }
    public decimal ProteinaPct { get; init; }
    public decimal EnergiaMcal { get; init; }
    public bool Activa { get; init; }
}
