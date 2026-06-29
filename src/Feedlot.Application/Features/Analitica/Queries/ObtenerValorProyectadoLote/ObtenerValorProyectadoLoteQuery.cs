using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Analitica.Queries.ObtenerValorProyectadoLote;

/// <summary>
/// Calcula el valor de venta proyectado de un lote usando los precios reales
/// de una subasta de SUBAGAN.
///
/// Para cada animal ACTIVO del lote:
///   valor_animal = peso_actual_kg × precio/kg del tipo comercial del animal en esa subasta
///
/// Se OMITEN los animales que:
///   - no tienen tipo comercial asignado, o
///   - cuyo tipo no aparece en la subasta elegida.
/// El resultado informa cuántos se incluyeron y cuántos se omitieron.
/// </summary>
public sealed record ObtenerValorProyectadoLoteQuery(
    Guid LoteId,
    Guid SubaganEventoId) : IRequest<Result<ValorProyectadoLoteDto>>;

public sealed record ValorProyectadoLoteDto(
    Guid LoteId,
    Guid SubaganEventoId,
    decimal ValorTotal,
    string Moneda,
    int AnimalesIncluidos,
    int AnimalesOmitidos,
    IReadOnlyList<ValorAnimalDto> Detalle);

public sealed record ValorAnimalDto(
    Guid AnimalId,
    string Codigo,
    string? Nombre,
    string? TipoComercial,
    decimal PesoActualKg,
    decimal? PrecioPorKg,
    decimal? ValorProyectado,
    bool Incluido,
    string? MotivoOmision);

public sealed class ObtenerValorProyectadoLoteQueryHandler
    : IRequestHandler<ObtenerValorProyectadoLoteQuery, Result<ValorProyectadoLoteDto>>
{
    private readonly ILoteRepository _loteRepo;
    private readonly IAnimalRepository _animalRepo;
    private readonly ISubaganEventoRepository _subaganRepo;

    public ObtenerValorProyectadoLoteQueryHandler(
        ILoteRepository loteRepo,
        IAnimalRepository animalRepo,
        ISubaganEventoRepository subaganRepo)
    {
        _loteRepo = loteRepo;
        _animalRepo = animalRepo;
        _subaganRepo = subaganRepo;
    }

    public async Task<Result<ValorProyectadoLoteDto>> Handle(
        ObtenerValorProyectadoLoteQuery request, CancellationToken ct)
    {
        var lote = await _loteRepo.ObtenerPorIdAsync(request.LoteId, ct);
        if (lote is null)
            return Result<ValorProyectadoLoteDto>.NotFound(
                $"No se encontró el lote {request.LoteId}.");

        var precios = await _subaganRepo.ObtenerPreciosPorTipoAsync(request.SubaganEventoId, ct);
        if (precios.Count == 0)
            return Result<ValorProyectadoLoteDto>.Failure(
                "La subasta seleccionada no tiene precios disponibles.",
                ResultErrorType.BusinessRule);

        // IDs de animales activos en el lote.
        var animalIds = lote.AnimalesLote
            .Where(al => al.EsActivo)
            .Select(al => al.AnimalId)
            .Distinct()
            .ToList();

        if (animalIds.Count == 0)
            return Result<ValorProyectadoLoteDto>.Success(new ValorProyectadoLoteDto(
                request.LoteId, request.SubaganEventoId, 0m, "COP", 0, 0, []));

        var animales = await _animalRepo.ObtenerPorIdsAsync(animalIds, ct);

        var detalle = new List<ValorAnimalDto>();
        decimal total = 0m;
        int incluidos = 0, omitidos = 0;

        foreach (var animal in animales)
        {
            var codigo = animal.CodigoIdentificacion.Valor;
            var pesoActual = animal.PesoActual.Kilogramos;
            var tipo = animal.TipoComercial?.ToString();

            if (tipo is null)
            {
                omitidos++;
                detalle.Add(new ValorAnimalDto(
                    animal.Id, codigo, animal.Nombre, null, pesoActual,
                    null, null, false, "Sin tipo comercial asignado"));
                continue;
            }

            if (!precios.TryGetValue(tipo, out var precioKg))
            {
                omitidos++;
                detalle.Add(new ValorAnimalDto(
                    animal.Id, codigo, animal.Nombre, tipo, pesoActual,
                    null, null, false, $"El tipo {tipo} no está en la subasta"));
                continue;
            }

            var valor = Math.Round(pesoActual * precioKg, 2);
            total += valor;
            incluidos++;
            detalle.Add(new ValorAnimalDto(
                animal.Id, codigo, animal.Nombre, tipo, pesoActual,
                precioKg, valor, true, null));
        }

        return Result<ValorProyectadoLoteDto>.Success(new ValorProyectadoLoteDto(
            request.LoteId,
            request.SubaganEventoId,
            Math.Round(total, 2),
            "COP",
            incluidos,
            omitidos,
            detalle));
    }
}
