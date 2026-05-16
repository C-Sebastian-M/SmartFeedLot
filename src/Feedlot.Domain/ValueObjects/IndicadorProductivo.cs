using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.ValueObjects;

/// <summary>
/// Value Object que encapsula los indicadores productivos calculados.
/// GMD, ICA, costo por kg ganado y rentabilidad proyectada.
/// Los cálculos son las fórmulas reales del feedlot — inmutables y auditables.
/// </summary>
public sealed class IndicadorProductivo : ValueObject
{
    /// <summary>Ganancia Media Diaria en kg/día.</summary>
    public decimal GananciaMediaDiaria { get; }

    /// <summary>Índice de Conversión Alimenticia: kg alimento / kg ganado.</summary>
    public decimal IndiceConversionAlimenticia { get; }

    /// <summary>Costo por kilogramo de peso ganado.</summary>
    public decimal CostoPorKgGanado { get; }

    /// <summary>Rentabilidad proyectada = precio venta estimado - costos totales.</summary>
    public decimal RentabilidadProyectada { get; }

    /// <summary>Días transcurridos en el período de cálculo.</summary>
    public int DiasEnEngorde { get; }

    private IndicadorProductivo(
        decimal gmd,
        decimal ica,
        decimal costoPorKgGanado,
        decimal rentabilidadProyectada,
        int diasEnEngorde)
    {
        GananciaMediaDiaria = gmd;
        IndiceConversionAlimenticia = ica;
        CostoPorKgGanado = costoPorKgGanado;
        RentabilidadProyectada = rentabilidadProyectada;
        DiasEnEngorde = diasEnEngorde;
    }

    /// <summary>
    /// Calcula los indicadores productivos aplicando las fórmulas del feedlot.
    /// GMD = (PesoFinal - PesoInicial) / Días
    /// ICA = AlimentoConsumido / PesoGanado
    /// CostoKg = CostoTotalAlimento / PesoGanado
    /// Rentabilidad = PrecioVentaEstimado - CostosTotales
    /// </summary>
    public static IndicadorProductivo Calcular(
        decimal pesoInicialKg,
        decimal pesoFinalKg,
        int diasEnEngorde,
        decimal alimentoConsumidoKg,
        decimal costoTotalAlimento,
        decimal precioVentaEstimado,
        decimal costosTotales)
    {
        if (diasEnEngorde <= 0)
            throw new DomainException(
                $"Los días en engorde deben ser mayores a cero para calcular indicadores. " +
                $"Recibido: {diasEnEngorde}.");

        if (pesoFinalKg <= 0 || pesoInicialKg <= 0)
            throw new DomainException("Los pesos inicial y final deben ser mayores a cero.");

        decimal pesoGanado = pesoFinalKg - pesoInicialKg;

        // Si el animal perdió peso, GMD será negativa — dato productivo válido (alerta).
        decimal gmd = pesoGanado / diasEnEngorde;

        // ICA y costo por kg solo son significativos si hubo ganancia de peso.
        decimal ica = pesoGanado > 0 && alimentoConsumidoKg > 0
            ? alimentoConsumidoKg / pesoGanado
            : 0;

        decimal costoPorKgGanado = pesoGanado > 0 && costoTotalAlimento > 0
            ? costoTotalAlimento / pesoGanado
            : 0;

        decimal rentabilidad = precioVentaEstimado - costosTotales;

        return new IndicadorProductivo(gmd, ica, costoPorKgGanado, rentabilidad, diasEnEngorde);
    }

    /// <summary>
    /// Determina si el animal es ineficiente según los umbrales del negocio.
    /// Un animal con GMD menor al umbral mínimo o ICA mayor al umbral máximo
    /// se considera ineficiente y debe generar una alerta.
    /// </summary>
    public bool EsIneficiente(decimal gmdMinimaKgDia, decimal icaMaxima)
        => GananciaMediaDiaria < gmdMinimaKgDia || (IndiceConversionAlimenticia > icaMaxima && IndiceConversionAlimenticia > 0);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GananciaMediaDiaria;
        yield return IndiceConversionAlimenticia;
        yield return CostoPorKgGanado;
        yield return RentabilidadProyectada;
        yield return DiasEnEngorde;
    }
}
