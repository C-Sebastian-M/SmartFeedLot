using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Costos.Queries.ObtenerCostosTotalesLote;

public sealed class ObtenerCostosTotalesLoteQueryHandler
    : IRequestHandler<ObtenerCostosTotalesLoteQuery, Result<CosteoLoteDto>>
{
    private readonly ILoteRepository _loteRepo;
    private readonly IConsumoAlimenticioRepository _consumoRepo;
    private readonly IMovimientoFinancieroRepository _movimientoRepo;

    public ObtenerCostosTotalesLoteQueryHandler(
        ILoteRepository loteRepo,
        IConsumoAlimenticioRepository consumoRepo,
        IMovimientoFinancieroRepository movimientoRepo)
    {
        _loteRepo = loteRepo;
        _consumoRepo = consumoRepo;
        _movimientoRepo = movimientoRepo;
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

        // Obtener todos los lotes activos para calcular población total y prorratear
        var lotesActivos = await _loteRepo.ObtenerActivosAsync(ct);
        decimal totalAnimalesGranja = lotesActivos.Sum(l => l.CantidadAnimalesActivos);
        if (totalAnimalesGranja == 0) totalAnimalesGranja = 1;

        // Obtener movimientos financieros del periodo para Bovino y General
        var movimientosBovino = await _movimientoRepo.ObtenerPorRangoFechasAsync(
            request.Desde, request.Hasta, OrigenFinanciero.Bovino, ct);
        var movimientosGeneral = await _movimientoRepo.ObtenerPorRangoFechasAsync(
            request.Desde, request.Hasta, OrigenFinanciero.General, ct);
        var todosMovimientos = movimientosBovino.Concat(movimientosGeneral).ToList();

        // ── Mano de obra ──────────────────────────────────────────────────────
        var detallesMo = todosMovimientos
            .Where(m => m.CategoriaGasto.Tipo == TipoCategoriaGasto.Operativo)
            .Select(m => {
                decimal proratedMonto = (m.Monto.Monto / totalAnimalesGranja) * totalAnimales;
                return new CostoDetalleDto
                {
                    Id = m.Id,
                    Categoria = m.CategoriaGasto.Nombre,
                    Concepto = m.Descripcion,
                    Fecha = m.Fecha,
                    Monto = Math.Round(proratedMonto, 2),
                    Moneda = m.Monto.Moneda,
                    Observaciones = $"Prorrateado de {m.Monto} registrado en {m.Origen}"
                };
            }).ToList();
        var costoMoTotal = detallesMo.Sum(c => c.Monto);

        // ── CIF ───────────────────────────────────────────────────────────────
        var detallesCif = todosMovimientos
            .Where(m => m.CategoriaGasto.Tipo == TipoCategoriaGasto.Indirecto)
            .Select(m => {
                decimal proratedMonto = (m.Monto.Monto / totalAnimalesGranja) * totalAnimales;
                return new CostoDetalleDto
                {
                    Id = m.Id,
                    Categoria = m.CategoriaGasto.Nombre,
                    Concepto = m.Descripcion,
                    Fecha = m.Fecha,
                    Monto = Math.Round(proratedMonto, 2),
                    Moneda = m.Monto.Moneda,
                    Observaciones = $"Prorrateado de {m.Monto} registrado en {m.Origen}"
                };
            }).ToList();
        var costoCifTotal = detallesCif.Sum(c => c.Monto);

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
            DetallesManoDeObra = detallesMo,

            CostoTotalCif = costoCifTotal,
            CostoCifPorAnimal = costoCifTotal / totalAnimales,
            DetallesCif = detallesCif,

            CostoOperativoTotal = costoOperativoTotal,
            CostoOperativoPorAnimal = costoOperativoTotal / totalAnimales,
        };

        return Result<CosteoLoteDto>.Success(dto);
    }
}
