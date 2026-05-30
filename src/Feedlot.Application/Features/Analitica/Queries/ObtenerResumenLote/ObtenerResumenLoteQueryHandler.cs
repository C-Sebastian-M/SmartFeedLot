using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.Services;
using MediatR;

namespace Feedlot.Application.Features.Analitica.Queries.ObtenerResumenLote;

public sealed class ObtenerResumenLoteQueryHandler
    : IRequestHandler<ObtenerResumenLoteQuery, Result<ResumenLoteDto>>
{
    private readonly ILoteRepository _loteRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly IConsumoAlimenticioRepository _consumoRepository;
    private readonly IndicadorProductivoService _indicadorService;

    private const decimal GmdMinimaKgDia = 0.8m;
    private const decimal IcaMaxima = 8.0m;

    public ObtenerResumenLoteQueryHandler(
        ILoteRepository loteRepository,
        IAnimalRepository animalRepository,
        IConsumoAlimenticioRepository consumoRepository,
        IndicadorProductivoService indicadorService)
    {
        _loteRepository = loteRepository;
        _animalRepository = animalRepository;
        _consumoRepository = consumoRepository;
        _indicadorService = indicadorService;
    }

    public async Task<Result<ResumenLoteDto>> Handle(
        ObtenerResumenLoteQuery request,
        CancellationToken ct)
    {
        var lote = await _loteRepository.ObtenerPorIdAsync(request.LoteId, ct);
        if (lote is null)
            return Result<ResumenLoteDto>.NotFound(
                $"No se encontró el lote con ID '{request.LoteId}'.");

        var animalesIdActivos = lote.AnimalesLote
            .Where(al => al.EsActivo)
            .Select(al => al.AnimalId)
            .ToList();

        var cantidadAnimales = Math.Max(animalesIdActivos.Count, 1);

        // ── OPTIMIZACIÓN N+1 ──────────────────────────────────────────────────
        // ANTES: N llamadas a ObtenerPorIdAsync + 4N queries de costos = ~5N queries.
        // AHORA: 1 query batch de animales + 4 queries de costos = 5 queries totales,
        //        sin importar cuántos animales tenga el lote.

        // 1. Cargar TODOS los animales del lote en una sola consulta.
        var animales = await _animalRepository.ObtenerPorIdsAsync(animalesIdActivos, ct);

        // 2. Cargar los costos del lote UNA sola vez (no por animal).
        var costosLote = await _indicadorService.ObtenerCostosLoteAsync(
            request.LoteId, request.Desde, request.Hasta, ct);

        // 3. Calcular indicadores en memoria — cero queries adicionales.
        var indicadores = new List<IndicadorProductivoDto>();

        foreach (var animal in animales)
        {
            var indicador = _indicadorService.CalcularConCostos(
                animal,
                cantidadAnimales,
                request.Desde,
                request.Hasta,
                request.PrecioVentaEstimadoPorKg,
                costosLote);

            var pesoGanado = animal.PesoActual.Kilogramos - animal.PesoIngreso.Kilogramos;
            var esIneficiente = indicador.EsIneficiente(GmdMinimaKgDia, IcaMaxima);

            indicadores.Add(new IndicadorProductivoDto
            {
                AnimalId = animal.Id,
                CodigoAnimal = animal.CodigoIdentificacion.Valor,
                NombreAnimal = animal.Nombre,
                Raza = animal.Raza,
                PesoInicialKg = animal.PesoIngreso.Kilogramos,
                PesoActualKg = animal.PesoActual.Kilogramos,
                PesoGanadoKg = pesoGanado,
                DiasEnEngorde = indicador.DiasEnEngorde,
                Gmd = indicador.GananciaMediaDiaria,
                Ica = indicador.IndiceConversionAlimenticia,
                CostoPorKgGanado = indicador.CostoPorKgGanado,
                RentabilidadProyectada = indicador.RentabilidadProyectada,
                EsIneficiente = esIneficiente,
                ClasificacionGmd = indicador.GananciaMediaDiaria switch
                {
                    >= 1.4m => "Excelente",
                    >= 1.1m => "Bueno",
                    >= 0.8m => "Regular",
                    _ => "Deficiente"
                }
            });
        }

        var resumen = new ResumenLoteDto
        {
            LoteId = lote.Id,
            CodigoLote = lote.Codigo,
            NombreLote = lote.Nombre,
            TotalAnimales = cantidadAnimales,
            GmdPromedioKgDia = indicadores.Count > 0
                ? indicadores.Average(i => i.Gmd) : 0,
            IcaPromedio = indicadores.Count > 0
                ? indicadores.Average(i => i.Ica) : 0,
            CostoTotalAlimento = costosLote.CostoAlimentoTotal,
            CostoPorKgGanadoPromedio = indicadores.Count > 0
                ? indicadores.Average(i => i.CostoPorKgGanado) : 0,
            RentabilidadProyectadaTotal = indicadores.Sum(i => i.RentabilidadProyectada),
            AnimalesIneficientes = indicadores.Count(i => i.EsIneficiente),
            ConsumoTotalKg = costosLote.ConsumoTotalKg,
            Indicadores = indicadores.AsReadOnly()
        };

        return Result<ResumenLoteDto>.Success(resumen);
    }
}
