using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Queries.ObtenerMovimientosFinancieros;

public sealed record ObtenerMovimientosFinancierosQuery(
    int? Anio = null,
    int? Mes = null,
    string? Origen = null,
    Guid? CategoriaGastoId = null,
    Guid? SocioId = null
) : IRequest<Result<IReadOnlyList<MovimientoFinancieroDto>>>;

public sealed class MovimientoFinancieroDto
{
    public Guid Id { get; init; }
    public DateOnly Fecha { get; init; }
    public int PeriodoAnio { get; init; }
    public int PeriodoMes { get; init; }
    public Guid CategoriaGastoId { get; init; }
    public string CategoriaGastoNombre { get; init; } = null!;
    public string CategoriaGastoTipo { get; init; } = null!;
    public decimal Monto { get; init; }
    public string Moneda { get; init; } = null!;
    public string Origen { get; init; } = null!;
    public string Descripcion { get; init; } = null!;
    public Guid? SocioId { get; init; }
    public string? SocioNombre { get; init; }
}

public sealed class ObtenerMovimientosFinancierosQueryHandler
    : IRequestHandler<ObtenerMovimientosFinancierosQuery, Result<IReadOnlyList<MovimientoFinancieroDto>>>
{
    private readonly IMovimientoFinancieroRepository _movimientoRepo;

    public ObtenerMovimientosFinancierosQueryHandler(IMovimientoFinancieroRepository movimientoRepo)
    {
        _movimientoRepo = movimientoRepo;
    }

    public async Task<Result<IReadOnlyList<MovimientoFinancieroDto>>> Handle(
        ObtenerMovimientosFinancierosQuery request,
        CancellationToken ct)
    {
        OrigenFinanciero? origen = null;
        if (!string.IsNullOrWhiteSpace(request.Origen))
        {
            if (Enum.TryParse<OrigenFinanciero>(request.Origen, ignoreCase: true, out var parsedOrigen))
            {
                origen = parsedOrigen;
            }
            else
            {
                return Result<IReadOnlyList<MovimientoFinancieroDto>>.Failure("Origen financiero inválido.");
            }
        }

        var movimientos = await _movimientoRepo.ObtenerPorFiltroAsync(
            request.Anio,
            request.Mes,
            origen,
            request.CategoriaGastoId,
            request.SocioId,
            ct);

        var dtos = movimientos.Select(m => new MovimientoFinancieroDto
        {
            Id = m.Id,
            Fecha = m.Fecha,
            PeriodoAnio = m.PeriodoAnio,
            PeriodoMes = m.PeriodoMes,
            CategoriaGastoId = m.CategoriaGastoId,
            CategoriaGastoNombre = m.CategoriaGasto.Nombre,
            CategoriaGastoTipo = m.CategoriaGasto.Tipo.ToString(),
            Monto = m.Monto.Monto,
            Moneda = m.Monto.Moneda,
            Origen = m.Origen.ToString(),
            Descripcion = m.Descripcion,
            SocioId = m.SocioId,
            SocioNombre = m.Socio?.Nombre
        }).ToList();

        return Result<IReadOnlyList<MovimientoFinancieroDto>>.Success(dtos);
    }
}
