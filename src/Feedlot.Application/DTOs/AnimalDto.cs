namespace Feedlot.Application.DTOs;

/// <summary>
/// DTO de respuesta para Animal. Expone solo lo necesario para el frontend.
/// Nunca expone el Aggregate directamente — eso acoplaría la API al modelo de dominio.
/// </summary>
public sealed class AnimalDto
{
    public Guid Id { get; init; }
    public string CodigoIdentificacion { get; init; } = null!;
    public string? Nombre { get; init; }
    public string NumeroArete { get; init; } = null!;
    public string Sexo { get; init; } = null!;
    public string Raza { get; init; } = null!;
    public DateOnly? FechaNacimiento { get; init; }
    public decimal PesoIngresoKg { get; init; }
    public decimal PrecioCompra { get; init; }
    public string Moneda { get; init; } = null!;
    public DateOnly FechaIngreso { get; init; }
    public string? TipoComercial { get; init; }
    public string EstadoProductivo { get; init; } = null!;
    public string EstadoSanitario { get; init; } = null!;
    public decimal PesoActualKg { get; init; }
    public int DiasEnEngorde { get; init; }
    public int TotalPesajes { get; init; }
    public List<PesajeDto> Pesajes { get; init; } = [];
    public List<EventoSanitarioDto> EventosSanitarios { get; init; } = [];
}

/// <summary>DTO resumido para listas y tablas del dashboard.</summary>
public sealed class AnimalResumenDto
{
    public Guid Id { get; init; }
    public string CodigoIdentificacion { get; init; } = null!;
    public string? Nombre { get; init; }
    public string NumeroArete { get; init; } = null!;
    public string Raza { get; init; } = null!;
    public string Sexo { get; init; } = null!;
    public decimal PesoActualKg { get; init; }
    public int DiasEnEngorde { get; init; }
    public string EstadoProductivo { get; init; } = null!;
    public string EstadoSanitario { get; init; } = null!;
    public string? TipoComercial { get; init; }
}
