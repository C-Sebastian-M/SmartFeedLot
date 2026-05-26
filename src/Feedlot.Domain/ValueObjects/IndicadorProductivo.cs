using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;

namespace Feedlot.Domain.ValueObjects;

/// <summary>
/// Value Object que encapsula el costeo completo y los indicadores productivos.
/// Refleja exactamente la estructura del Excel:
///   TOTAL COSTO UNITARIO = Precio Animal + Alimento + Mano de Obra + CIF
///   GMD = (PesoFinal - PesoInicial) / Días
///   ICA = AlimentoConsumido / PesoGanado
///   CostoKg = CostoAlimento / PesoGanado
///   Rentabilidad = PrecioVenta - CostoTotalUnitario
/// </summary>
public sealed class IndicadorProductivo : ValueObject
{
    /// <summary>Ganancia Media Diaria en kg/día.</summary>
    public decimal GananciaMediaDiaria { get; }

    /// <summary>Índice de Conversión Alimenticia: kg alimento / kg ganado.</summary>
    public decimal IndiceConversionAlimenticia { get; }

    /// <summary>Costo por kilogramo ganado (solo alimento).</summary>
    public decimal CostoPorKgGanado { get; }

    /// <summary>
    /// Rentabilidad = PrecioVentaEstimado - CostoTotalUnitario (incluye MO y CIF).
    /// </summary>
    public decimal RentabilidadProyectada { get; }

    /// <summary>Días transcurridos en el período de cálculo.</summary>
    public int DiasEnEngorde { get; }

    // ── Desglose del costo unitario (columnas del Excel) ─────────────────────

    /// <summary>Costo de alimento prorrateado al animal.</summary>
    public decimal CostoAlimentoIndividual { get; }

    /// <summary>Mano de obra prorrateada al animal.</summary>
    public decimal CostoManoDeObraIndividual { get; }

    /// <summary>CIF prorrateado al animal.</summary>
    public decimal CostoCifIndividual { get; }

    /// <summary>
    /// Costo total unitario = PrecioCompraAnimal + Alimento + MO + CIF.
    /// Equivale a "TOTAL COSTO UNITARIO" del Excel.
    /// </summary>
    public decimal CostoTotalUnitario { get; }

    private IndicadorProductivo(
        decimal gmd,
        decimal ica,
        decimal costoPorKgGanado,
        decimal rentabilidad,
        int diasEnEngorde,
        decimal costoAlimento,
        decimal costoMo,
        decimal costoCif,
        decimal costoTotal)
    {
        GananciaMediaDiaria = gmd;
        IndiceConversionAlimenticia = ica;
        CostoPorKgGanado = costoPorKgGanado;
        RentabilidadProyectada = rentabilidad;
        DiasEnEngorde = diasEnEngorde;
        CostoAlimentoIndividual = costoAlimento;
        CostoManoDeObraIndividual = costoMo;
        CostoCifIndividual = costoCif;
        CostoTotalUnitario = costoTotal;
    }

    public static IndicadorProductivo Calcular(
        decimal pesoInicialKg,
        decimal pesoFinalKg,
        int diasEnEngorde,
        decimal alimentoConsumidoKg,
        decimal costoAlimento,
        decimal precioVentaEstimado,
        decimal costoTotalUnitario,
        decimal costoMo = 0,
        decimal costoCif = 0)
    {
        if (diasEnEngorde <= 0)
            throw new DomainException(
                $"Los días en engorde deben ser mayores a cero. Recibido: {diasEnEngorde}.");

        decimal pesoGanado = pesoFinalKg - pesoInicialKg;
        decimal gmd = pesoGanado / diasEnEngorde;

        decimal ica = pesoGanado > 0 && alimentoConsumidoKg > 0
            ? alimentoConsumidoKg / pesoGanado : 0;

        decimal costoPorKgGanado = pesoGanado > 0 && costoAlimento > 0
            ? costoAlimento / pesoGanado : 0;

        decimal rentabilidad = precioVentaEstimado - costoTotalUnitario;

        return new IndicadorProductivo(
            gmd, ica, costoPorKgGanado, rentabilidad, diasEnEngorde,
            costoAlimento, costoMo, costoCif, costoTotalUnitario);
    }

    public bool EsIneficiente(decimal gmdMinimaKgDia, decimal icaMaxima)
        => GananciaMediaDiaria < gmdMinimaKgDia
           || (IndiceConversionAlimenticia > icaMaxima && IndiceConversionAlimenticia > 0);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GananciaMediaDiaria;
        yield return IndiceConversionAlimenticia;
        yield return CostoPorKgGanado;
        yield return RentabilidadProyectada;
        yield return DiasEnEngorde;
        yield return CostoTotalUnitario;
    }
}
