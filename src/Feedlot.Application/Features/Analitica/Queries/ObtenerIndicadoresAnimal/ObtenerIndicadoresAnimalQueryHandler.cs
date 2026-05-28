using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.Services;
using MediatR;

namespace Feedlot.Application.Features.Analitica.Queries.ObtenerIndicadoresAnimal;

public sealed class ObtenerIndicadoresAnimalQueryHandler
    : IRequestHandler<ObtenerIndicadoresAnimalQuery, Result<IndicadorProductivoDto>>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly ILoteRepository _loteRepository;
    private readonly IndicadorProductivoService _indicadorService;

    // Umbrales de referencia del negocio feedlot bovino.
    // En una versión futura, estos valores vendrán de una entidad de configuración.
    private const decimal GmdMinimaKgDia = 0.8m;
    private const decimal IcaMaxima = 8.0m;

    public ObtenerIndicadoresAnimalQueryHandler(
        IAnimalRepository animalRepository,
        ILoteRepository loteRepository,
        IndicadorProductivoService indicadorService)
    {
        _animalRepository = animalRepository;
        _loteRepository = loteRepository;
        _indicadorService = indicadorService;
    }

    public async Task<Result<IndicadorProductivoDto>> Handle(
        ObtenerIndicadoresAnimalQuery request,
        CancellationToken ct)
    {
        var animal = await _animalRepository.ObtenerPorIdAsync(request.AnimalId, ct);
        if (animal is null)
            return Result<IndicadorProductivoDto>.NotFound(
                $"No se encontró el animal con ID '{request.AnimalId}'.");

        var lote = await _loteRepository.ObtenerPorIdAsync(request.LoteId, ct);
        if (lote is null)
            return Result<IndicadorProductivoDto>.NotFound(
                $"No se encontró el lote con ID '{request.LoteId}'.");

        var cantidadAnimales = Math.Max(lote.CantidadAnimalesActivos, 1);

        var indicador = await _indicadorService.CalcularParaAnimalAsync(
            animal,
            request.LoteId,
            cantidadAnimales,
            request.Desde,
            request.Hasta,
            request.PrecioVentaEstimadoPorKg,
            ct);

        var pesoGanado = animal.PesoActual.Kilogramos - animal.PesoIngreso.Kilogramos;
        var esIneficiente = indicador.EsIneficiente(GmdMinimaKgDia, IcaMaxima);

        var dto = new IndicadorProductivoDto
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
            ClasificacionGmd = ClasificarGmd(indicador.GananciaMediaDiaria)
        };

        return Result<IndicadorProductivoDto>.Success(dto);
    }

    private static string ClasificarGmd(decimal gmd) => gmd switch
    {
        >= 1.4m => "Excelente",
        >= 1.1m => "Bueno",
        >= 0.8m => "Regular",
        _ => "Deficiente"
    };
}
