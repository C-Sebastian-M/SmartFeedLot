using Feedlot.Application.Common;
using Feedlot.Application.DTOs;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.Services;
using MediatR;

namespace Feedlot.Application.Features.Analitica.Queries.ObtenerAnimalesIneficientes;

public sealed class ObtenerAnimalesIneficientesQueryHandler
    : IRequestHandler<ObtenerAnimalesIneficientesQuery, Result<IReadOnlyList<AnimalIneficienteDto>>>
{
    private readonly ILoteRepository _loteRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly IndicadorProductivoService _indicadorService;

    public ObtenerAnimalesIneficientesQueryHandler(
        ILoteRepository loteRepository,
        IAnimalRepository animalRepository,
        IndicadorProductivoService indicadorService)
    {
        _loteRepository = loteRepository;
        _animalRepository = animalRepository;
        _indicadorService = indicadorService;
    }

    public async Task<Result<IReadOnlyList<AnimalIneficienteDto>>> Handle(
        ObtenerAnimalesIneficientesQuery request,
        CancellationToken ct)
    {
        // Si se especificó un lote, analizar solo ese. Si no, analizar todos los activos.
        var lotes = request.LoteId.HasValue
            ? new[] { await _loteRepository.ObtenerPorIdAsync(request.LoteId.Value, ct) }
                .Where(l => l is not null)
                .Select(l => l!)
                .ToList()
            : (await _loteRepository.ObtenerActivosAsync(ct)).ToList();

        var ineficientes = new List<AnimalIneficienteDto>();

        foreach (var lote in lotes)
        {
            var animalesActivos = lote.AnimalesLote
                .Where(al => al.EsActivo)
                .ToList();

            var cantidadAnimales = Math.Max(animalesActivos.Count, 1);

            foreach (var animalLote in animalesActivos)
            {
                var animal = await _animalRepository
                    .ObtenerPorIdAsync(animalLote.AnimalId, ct);

                if (animal is null || !animal.EstaActivo) continue;

                var indicador = await _indicadorService.CalcularParaAnimalAsync(
                    animal,
                    lote.Id,
                    cantidadAnimales,
                    request.Desde,
                    request.Hasta,
                    request.PrecioVentaEstimadoPorKg,
                    ct);

                if (!indicador.EsIneficiente(request.GmdMinimaKgDia, request.IcaMaxima))
                    continue;

                var motivos = new List<string>();
                if (indicador.GananciaMediaDiaria < request.GmdMinimaKgDia)
                    motivos.Add($"GMD {indicador.GananciaMediaDiaria:F2} < mínimo {request.GmdMinimaKgDia:F2} kg/día");
                if (indicador.IndiceConversionAlimenticia > request.IcaMaxima && indicador.IndiceConversionAlimenticia > 0)
                    motivos.Add($"ICA {indicador.IndiceConversionAlimenticia:F2} > máximo {request.IcaMaxima:F2}");

                ineficientes.Add(new AnimalIneficienteDto
                {
                    AnimalId = animal.Id,
                    CodigoAnimal = animal.CodigoIdentificacion.Valor,
                    NombreAnimal = animal.Nombre,
                    Raza = animal.Raza,
                    LoteCodigo = lote.Codigo,
                    Gmd = indicador.GananciaMediaDiaria,
                    GmdMinimaEsperada = request.GmdMinimaKgDia,
                    Ica = indicador.IndiceConversionAlimenticia,
                    IcaMaximaEsperada = request.IcaMaxima,
                    DiasEnEngorde = indicador.DiasEnEngorde,
                    MotivoAlerta = string.Join(" | ", motivos)
                });
            }
        }

        // Ordenar por GMD ascendente: los más críticos primero.
        var ordenados = ineficientes
            .OrderBy(a => a.Gmd)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<AnimalIneficienteDto>>.Success(ordenados);
    }
}
