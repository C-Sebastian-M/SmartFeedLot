using Feedlot.Domain.Entities;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Services;

/// <summary>
/// Domain Service que calcula indicadores productivos para un animal.
/// 
/// Coordina datos de Animal (pesajes) y ConsumoAlimenticio (alimento del lote)
/// para producir el IndicadorProductivo. Ambas fuentes son aggregates distintos,
/// por eso la lógica vive en un Domain Service y no en ninguno de los dos.
/// 
/// Los cálculos son determinísticos y auditables: las mismas entradas
/// siempre producen el mismo resultado.
/// </summary>
public sealed class IndicadorProductivoService
{
    private readonly IConsumoAlimenticioRepository _consumoRepo;

    public IndicadorProductivoService(IConsumoAlimenticioRepository consumoRepo)
    {
        _consumoRepo = consumoRepo;
    }

    /// <summary>
    /// Calcula los indicadores productivos de un animal en un período dado.
    /// 
    /// El consumo alimenticio es a nivel de lote (suministro colectivo).
    /// Para estimar el consumo individual, se divide el consumo del lote
    /// por el número de animales activos en ese período.
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
                "La cantidad de animales en el lote debe ser mayor a cero para distribuir el consumo.");

        int dias = hasta.DayNumber - desde.DayNumber;

        // Obtener el pesaje más cercano a 'desde' como peso inicial del período.
        var pesajeInicial = animal.Pesajes
            .Where(p => p.FechaPesaje <= desde)
            .OrderByDescending(p => p.FechaPesaje)
            .FirstOrDefault();

        // Obtener el pesaje más cercano a 'hasta' como peso final del período.
        var pesajeFinal = animal.Pesajes
            .Where(p => p.FechaPesaje <= hasta)
            .OrderByDescending(p => p.FechaPesaje)
            .FirstOrDefault();

        decimal pesoInicialKg = pesajeInicial?.Peso.Kilogramos ?? animal.PesoIngreso.Kilogramos;
        decimal pesoFinalKg = pesajeFinal?.Peso.Kilogramos ?? animal.PesoIngreso.Kilogramos;

        // Consumo total del lote en el período dividido por animales (estimación individual).
        decimal consumoTotalLoteKg = await _consumoRepo.SumarKilogramosPorLoteAsync(
            loteId, desde, hasta, ct);

        decimal costoTotalLote = await _consumoRepo.SumarCostoPorLoteAsync(
            loteId, desde, hasta, ct);

        decimal consumoIndividualKg = consumoTotalLoteKg / cantidadAnimalesEnLote;
        decimal costoIndividual = costoTotalLote / cantidadAnimalesEnLote;

        decimal pesoGanado = pesoFinalKg - pesoInicialKg;
        decimal precioVentaEstimado = pesoFinalKg * precioVentaEstimadoPorKg;
        decimal costosTotales = costoIndividual + animal.PrecioCompra.Monto;

        return IndicadorProductivo.Calcular(
            pesoInicialKg,
            pesoFinalKg,
            dias > 0 ? dias : 1,
            consumoIndividualKg,
            costoIndividual,
            precioVentaEstimado,
            costosTotales);
    }
}
