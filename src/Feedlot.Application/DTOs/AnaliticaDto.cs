namespace Feedlot.Application.DTOs;

/// <summary>
/// DTO de indicadores productivos individuales por animal.
/// Proyectado desde IndicadorProductivo (Value Object del dominio).
/// </summary>
public sealed class IndicadorProductivoDto
{
    public Guid AnimalId { get; init; }
    public string CodigoAnimal { get; init; } = null!;
    public string? NombreAnimal { get; init; }
    public string Raza { get; init; } = null!;
    public decimal PesoInicialKg { get; init; }
    public decimal PesoActualKg { get; init; }
    public decimal PesoGanadoKg { get; init; }
    public int DiasEnEngorde { get; init; }

    /// <summary>Ganancia Media Diaria en kg/día.</summary>
    public decimal Gmd { get; init; }

    /// <summary>Índice de Conversión Alimenticia: kg alimento / kg ganado.</summary>
    public decimal Ica { get; init; }

    /// <summary>Costo por kilogramo ganado.</summary>
    public decimal CostoPorKgGanado { get; init; }

    /// <summary>Rentabilidad proyectada.</summary>
    public decimal RentabilidadProyectada { get; init; }

    /// <summary>Si el animal está por debajo del umbral productivo.</summary>
    public bool EsIneficiente { get; init; }

    /// <summary>Clasificación visual: Excelente / Bueno / Regular / Deficiente.</summary>
    public string ClasificacionGmd { get; init; } = null!;
}

/// <summary>
/// Resumen ejecutivo de un lote para el dashboard principal.
/// Agrega los indicadores de todos los animales del lote.
/// </summary>
public sealed class ResumenLoteDto
{
    public Guid LoteId { get; init; }
    public string CodigoLote { get; init; } = null!;
    public string NombreLote { get; init; } = null!;
    public int TotalAnimales { get; init; }
    public decimal GmdPromedioKgDia { get; init; }
    public decimal IcaPromedio { get; init; }
    public decimal CostoTotalAlimento { get; init; }
    public decimal CostoPorKgGanadoPromedio { get; init; }
    public decimal RentabilidadProyectadaTotal { get; init; }
    public int AnimalesIneficientes { get; init; }
    public decimal ConsumoTotalKg { get; init; }
    public IReadOnlyList<IndicadorProductivoDto> Indicadores { get; init; } = [];
}

/// <summary>Animales que están por debajo de los umbrales productivos mínimos.</summary>
public sealed class AnimalIneficienteDto
{
    public Guid AnimalId { get; init; }
    public string CodigoAnimal { get; init; } = null!;
    public string? NombreAnimal { get; init; }
    public string Raza { get; init; } = null!;
    public string LoteCodigo { get; init; } = null!;
    public decimal Gmd { get; init; }
    public decimal GmdMinimaEsperada { get; init; }
    public decimal Ica { get; init; }
    public decimal IcaMaximaEsperada { get; init; }
    public int DiasEnEngorde { get; init; }
    public string MotivoAlerta { get; init; } = null!;
}
