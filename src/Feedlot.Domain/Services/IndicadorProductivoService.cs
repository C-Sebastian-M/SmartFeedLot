using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Services;

/// <summary>
/// Domain Service que calcula el costeo completo de un animal:
/// Materia Prima (alimento) + Mano de Obra + CIF + Precio compra animal.
///
/// Fórmulas del Excel implementadas:
///   TOTAL MATERIA PRIMA = precio animal + sal + melaza + vacunas + purgante + vitaminas
///   TOTAL MANO DE OBRA  = (costo_mes × meses) / n_animales
///   TOTAL CIF           = (gasolina + grama_fin + cal + urea + alquiler) / n_animales
///   COSTO UNITARIO TOTAL = MP + MO prorrateada + CIF prorrateado
///   GMD = (PesoFinal - PesoInicial) / Días
///   ICA = AlimentoConsumido / PesoGanado
///   CostoKgGanado = CostoTotalAlimento / PesoGanado
///   Rentabilidad = PrecioVentaEstimado - CostosUnitarioTotal
/// </summary>
public sealed class IndicadorProductivoService
{
    private readonly IConsumoAlimenticioRepository _consumoRepo;
    private readonly ICostoOperativoRepository _costoOperativoRepo;

    public IndicadorProductivoService(
        IConsumoAlimenticioRepository consumoRepo,
        ICostoOperativoRepository costoOperativoRepo)
    {
        _consumoRepo = consumoRepo;
        _costoOperativoRepo = costoOperativoRepo;
    }

    /// <summary>
    /// Calcula todos los indicadores productivos y el costeo completo de un animal
    /// en un período dado, distribuyendo MO y CIF del lote proporcionalmente.
    /// </summary>
    public async Task<IndicadorProductivo> CalcularParaAnimalAsync(
        Animal animal,
        Guid loteId,
        int cantidadAnimalesEnLote,
        DateOnly desde,
        DateOnly hasta,
        decimal precioVentaEstimadoPorKg,
        CancellationToken ct = default)
    {
        if (hasta <= desde)
            throw new DomainException(
                $"La fecha 'hasta' ({hasta}) debe ser posterior a 'desde' ({desde}).");

        if (cantidadAnimalesEnLote <= 0)
            throw new DomainException(
                "La cantidad de animales en el lote debe ser mayor a cero.");

        // Solución correcta
        int dias = Math.Max(hasta.DayNumber - desde.DayNumber, 1);

        // ── Peso inicial y final del período ──────────────────────────────────
        var pesajeInicial = animal.Pesajes
            .Where(p => p.FechaPesaje <= desde)
            .OrderByDescending(p => p.FechaPesaje)
            .FirstOrDefault();

        var pesajeFinal = animal.Pesajes
            .Where(p => p.FechaPesaje <= hasta)
            .OrderByDescending(p => p.FechaPesaje)
            .FirstOrDefault();

        decimal pesoInicialKg = pesajeInicial?.Peso.Kilogramos
            ?? animal.PesoIngreso.Kilogramos;
        decimal pesoFinalKg = pesajeFinal?.Peso.Kilogramos
            ?? animal.PesoIngreso.Kilogramos;

        // ── Materia prima: alimento del lote / n animales ────────────────────
        decimal consumoTotalLoteKg = await _consumoRepo
            .SumarKilogramosPorLoteAsync(loteId, desde, hasta, ct);
        decimal costoAlimentoLote = await _consumoRepo
            .SumarCostoPorLoteAsync(loteId, desde, hasta, ct);

        decimal consumoIndividualKg = consumoTotalLoteKg / cantidadAnimalesEnLote;
        decimal costoAlimentoIndividual = costoAlimentoLote / cantidadAnimalesEnLote;

        // ── Mano de obra: total MO del lote / n animales ─────────────────────
        decimal costoMoLote = await _costoOperativoRepo.SumarMontoPorLoteAsync(
            loteId, desde, hasta, CategoriaCosto.ManoDeObra, ct);
        decimal costoMoIndividual = costoMoLote / cantidadAnimalesEnLote;

        // ── CIF: total CIF del lote / n animales ─────────────────────────────
        decimal costoCifLote = await _costoOperativoRepo.SumarMontoPorLoteAsync(
            loteId, desde, hasta, CategoriaCosto.CIF, ct);
        decimal costoCifIndividual = costoCifLote / cantidadAnimalesEnLote;

        // ── Costo total unitario (como en el Excel) ───────────────────────────
        decimal costoTotalIndividual =
            animal.PrecioCompra.Monto  // Precio de compra del animal
            + costoAlimentoIndividual  // Materia prima (alimento)
            + costoMoIndividual        // Mano de obra prorrateada
            + costoCifIndividual;      // CIF prorrateado

        // ── Fórmulas productivas ──────────────────────────────────────────────
        decimal pesoGanado = pesoFinalKg - pesoInicialKg;
        decimal gmd = pesoGanado / dias;

        decimal ica = pesoGanado > 0 && consumoIndividualKg > 0
            ? consumoIndividualKg / pesoGanado : 0;

        decimal costoPorKgGanado = pesoGanado > 0 && costoAlimentoIndividual > 0
            ? costoAlimentoIndividual / pesoGanado : 0;

        decimal precioVentaEstimado = pesoFinalKg * precioVentaEstimadoPorKg;
        decimal rentabilidad = precioVentaEstimado - costoTotalIndividual;

        return IndicadorProductivo.Calcular(
            pesoInicialKg,
            pesoFinalKg,
            dias,
            consumoIndividualKg,
            costoAlimentoIndividual,
            precioVentaEstimado,
            costoTotalIndividual,
            costoMoIndividual,
            costoCifIndividual);
    }

    /// <summary>
    /// Costos de un lote en un período, ya consultados una sola vez.
    /// Se calculan una vez por lote y se reutilizan para todos sus animales,
    /// evitando el problema N+1 (4 queries idénticas por cada animal).
    /// </summary>
    public sealed record CostosLote(
        decimal ConsumoTotalKg,
        decimal CostoAlimentoTotal,
        decimal CostoManoDeObraTotal,
        decimal CostoCifTotal);

    /// <summary>
    /// Carga los costos de un lote (alimento, MO, CIF) en 4 consultas totales,
    /// independientemente de cuántos animales tenga el lote.
    /// Llamar UNA vez por lote y pasar el resultado a CalcularConCostos.
    /// </summary>
    public async Task<CostosLote> ObtenerCostosLoteAsync(
        Guid loteId,
        DateOnly desde,
        DateOnly hasta,
        CancellationToken ct = default)
    {
        var consumoKg = await _consumoRepo.SumarKilogramosPorLoteAsync(loteId, desde, hasta, ct);
        var costoAlimento = await _consumoRepo.SumarCostoPorLoteAsync(loteId, desde, hasta, ct);
        var costoMo = await _costoOperativoRepo.SumarMontoPorLoteAsync(
            loteId, desde, hasta, CategoriaCosto.ManoDeObra, ct);
        var costoCif = await _costoOperativoRepo.SumarMontoPorLoteAsync(
            loteId, desde, hasta, CategoriaCosto.CIF, ct);

        return new CostosLote(consumoKg, costoAlimento, costoMo, costoCif);
    }

    /// <summary>
    /// Calcula los indicadores de un animal usando costos del lote pre-cargados.
    /// NO accede a la base de datos: es pura computación en memoria.
    /// Esta es la versión que se usa en bucles sobre los animales de un lote.
    /// </summary>
    public IndicadorProductivo CalcularConCostos(
        Animal animal,
        int cantidadAnimalesEnLote,
        DateOnly desde,
        DateOnly hasta,
        decimal precioVentaEstimadoPorKg,
        CostosLote costosLote)
    {
        if (hasta <= desde)
            throw new DomainException(
                $"La fecha 'hasta' ({hasta}) debe ser posterior a 'desde' ({desde}).");

        if (cantidadAnimalesEnLote <= 0)
            throw new DomainException(
                "La cantidad de animales en el lote debe ser mayor a cero.");

        int dias = Math.Max(hasta.DayNumber - desde.DayNumber, 1);

        var pesajeInicial = animal.Pesajes
            .Where(p => p.FechaPesaje <= desde)
            .OrderByDescending(p => p.FechaPesaje)
            .FirstOrDefault();

        var pesajeFinal = animal.Pesajes
            .Where(p => p.FechaPesaje <= hasta)
            .OrderByDescending(p => p.FechaPesaje)
            .FirstOrDefault();

        decimal pesoInicialKg = pesajeInicial?.Peso.Kilogramos
            ?? animal.PesoIngreso.Kilogramos;
        decimal pesoFinalKg = pesajeFinal?.Peso.Kilogramos
            ?? animal.PesoIngreso.Kilogramos;

        // Prorrateo de costos del lote (ya cargados) entre los animales.
        decimal consumoIndividualKg = costosLote.ConsumoTotalKg / cantidadAnimalesEnLote;
        decimal costoAlimentoIndividual = costosLote.CostoAlimentoTotal / cantidadAnimalesEnLote;
        decimal costoMoIndividual = costosLote.CostoManoDeObraTotal / cantidadAnimalesEnLote;
        decimal costoCifIndividual = costosLote.CostoCifTotal / cantidadAnimalesEnLote;

        decimal costoTotalIndividual =
            animal.PrecioCompra.Monto
            + costoAlimentoIndividual
            + costoMoIndividual
            + costoCifIndividual;

        decimal precioVentaEstimado = pesoFinalKg * precioVentaEstimadoPorKg;

        return IndicadorProductivo.Calcular(
            pesoInicialKg,
            pesoFinalKg,
            dias,
            consumoIndividualKg,
            costoAlimentoIndividual,
            precioVentaEstimado,
            costoTotalIndividual,
            costoMoIndividual,
            costoCifIndividual);
    }
}
