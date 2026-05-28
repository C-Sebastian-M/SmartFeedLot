namespace Feedlot.Application.DTOs;

public sealed class LoteDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = null!;
    public string Nombre { get; init; } = null!;
    public int CapacidadMaxima { get; init; }
    public int AnimalesActuales { get; init; }
    public decimal PorcentajeOcupacion { get; init; }
    public string Estado { get; init; } = null!;
    public IReadOnlyList<AnimalLoteDto> Animales { get; init; } = [];
}

public sealed class LoteResumenDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = null!;
    public string Nombre { get; init; } = null!;
    public int CapacidadMaxima { get; init; }
    public int AnimalesActuales { get; init; }
    public decimal PorcentajeOcupacion { get; init; }
    public string Estado { get; init; } = null!;
}

public sealed class AnimalLoteDto
{
    public Guid AnimalId { get; init; }
    public string CodigoAnimal { get; set; } = null!;
    public string? NombreAnimal { get; set; }
    public DateOnly FechaIngreso { get; init; }
    public DateOnly? FechaEgreso { get; init; }
    public string MotivoIngreso { get; init; } = null!;
    public bool EsActivo { get; init; }
    public int DiasEnLote { get; init; }
}
