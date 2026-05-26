using Feedlot.Application.Common;
using Feedlot.Application.Features.Costos.Queries.ObtenerCostosTotalesLote;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Costos.Queries.ObtenerCostosTotalesLote;

public sealed class ObtenerCostosTotalesLoteQueryHandler
    : IRequestHandler<ObtenerCostosTotalesLoteQuery, Result<CosteoLoteDto>>
{
    private readonly ILoteRepository _loteRepo;
    private readonly IConsumoAlimenticioRepository _consumoRepo;
    private readonly ICostoOperativoRepository _costoRepo;

    public ObtenerCostosTotalesLoteQueryHandler(
        ILoteRepository loteRepo,
        IConsumoAlimenticioRepository consumoRepo,
        ICostoOperativoRepository costoRepo)
    {
        _loteRepo = loteRepo;
        _consumoRepo = consumoRepo;
        _costoRepo = costoRepo;
    }

    public async Task<Result<CosteoLoteDto>> Handle(
        ObtenerCostosTotalesLoteQuery request,
        CancellationToken ct)
    {
        var lote = await _loteRepo.ObtenerPorIdAsync(request.LoteId, ct);
        if (lote is null)
            return Result<CosteoLoteDto>.NotFound(
                $"No se encontró el lote con ID '{request.LoteId}'.");

        var totalAnimales = Math.Max(lote.CantidadAnimalesActivos, 1);

        // ── Alimento ──────────────────────────────────────────────────────────
        var consumoKg = await _consumoRepo.SumarKilogramosPorLoteAsync(
            request.LoteId, request.Desde, request.Hasta, ct);
        var costoAlimento = await _consumoRepo.SumarCostoPorLoteAsync(
            request.LoteId, request.Desde, request.Hasta, ct);

        // ── Mano de obra ──────────────────────────────────────────────────────
        var detallesMo = await _costoRepo.ObtenerPorLoteAsync(
            request.LoteId, request.Desde, request.Hasta, CategoriaCosto.ManoDeObra, ct);
        var costoMoTotal = detallesMo.Sum(c => c.Monto.Monto);

        // ── CIF ───────────────────────────────────────────────────────────────
        var detallesCif = await _costoRepo.ObtenerPorLoteAsync(
            request.LoteId, request.Desde, request.Hasta, CategoriaCosto.CIF, ct);
        var costoCifTotal = detallesCif.Sum(c => c.Monto.Monto);

        // ── Totales ───────────────────────────────────────────────────────────
        var costoOperativoTotal = costoAlimento + costoMoTotal + costoCifTotal;

        var dto = new CosteoLoteDto
        {
            LoteId = lote.Id,
            CodigoLote = lote.Codigo,
            TotalAnimales = totalAnimales,
            Desde = request.Desde,
            Hasta = request.Hasta,

            CostoTotalAlimento = costoAlimento,
            CostoAlimentoPorAnimal = costoAlimento / totalAnimales,
            ConsumoTotalKg = consumoKg,

            CostoTotalManoDeObra = costoMoTotal,
            CostoManoDeObraPorAnimal = costoMoTotal / totalAnimales,
            DetallesManoDeObra = detallesMo.Select(c => new CostoDetalleDto
            {
                Id = c.Id,
                Categoria = c.Categoria.ToString(),
                Concepto = c.Concepto,
                Fecha = c.Fecha,
                Monto = c.Monto.Monto,
                Moneda = c.Monto.Moneda,
                Observaciones = c.Observaciones,
            }).ToList(),

            CostoTotalCif = costoCifTotal,
            CostoCifPorAnimal = costoCifTotal / totalAnimales,
            DetallesCif = detallesCif.Select(c => new CostoDetalleDto
            {
                Id = c.Id,
                Categoria = c.Categoria.ToString(),
                Concepto = c.Concepto,
                Fecha = c.Fecha,
                Monto = c.Monto.Monto,
                Moneda = c.Monto.Moneda,
                Observaciones = c.Observaciones,
            }).ToList(),

            CostoOperativoTotal = costoOperativoTotal,
            CostoOperativoPorAnimal = costoOperativoTotal / totalAnimales,
        };

        return Result<CosteoLoteDto>.Success(dto);
    }
}
