using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Queries.ObtenerPrestamos;

public sealed record ObtenerPrestamosQuery : IRequest<Result<IReadOnlyList<PrestamoDto>>>;

public sealed class PrestamoDto
{
    public Guid Id { get; init; }
    public decimal Capital { get; init; }
    public string Moneda { get; init; } = null!;
    public decimal TasaMensual { get; init; }
    public int NCuotas { get; init; }
    public DateOnly FechaInicio { get; init; }
    public string Descripcion { get; init; } = null!;
    public IReadOnlyList<CuotaAmortizacionDto> Cuotas { get; init; } = [];
}

public sealed class CuotaAmortizacionDto
{
    public Guid Id { get; init; }
    public int NumeroCuota { get; init; }
    public DateOnly FechaVencimiento { get; init; }
    public decimal Cuota { get; init; }
    public decimal Interes { get; init; }
    public decimal AbonoCapital { get; init; }
    public decimal SaldoPendiente { get; init; }
    public bool Pagada { get; init; }
    public DateOnly? FechaPago { get; init; }
}

public sealed class ObtenerPrestamosQueryHandler
    : IRequestHandler<ObtenerPrestamosQuery, Result<IReadOnlyList<PrestamoDto>>>
{
    private readonly IPrestamoRepository _prestamoRepo;

    public ObtenerPrestamosQueryHandler(IPrestamoRepository prestamoRepo)
    {
        _prestamoRepo = prestamoRepo;
    }

    public async Task<Result<IReadOnlyList<PrestamoDto>>> Handle(
        ObtenerPrestamosQuery request,
        CancellationToken ct)
    {
        var prestamos = await _prestamoRepo.ObtenerTodosAsync(ct);
        var dtos = prestamos.Select(p => new PrestamoDto
        {
            Id = p.Id,
            Capital = p.Capital.Monto,
            Moneda = p.Capital.Moneda,
            TasaMensual = p.TasaMensual,
            NCuotas = p.NCuotas,
            FechaInicio = p.FechaInicio,
            Descripcion = p.Descripcion,
            Cuotas = p.Cuotas.Select(c => new CuotaAmortizacionDto
            {
                Id = c.Id,
                NumeroCuota = c.NumeroCuota,
                FechaVencimiento = c.FechaVencimiento,
                Cuota = c.Cuota.Monto,
                Interes = c.Interes.Monto,
                AbonoCapital = c.AbonoCapital.Monto,
                SaldoPendiente = c.SaldoPendiente.Monto,
                Pagada = c.Pagada,
                FechaPago = c.FechaPago
            }).OrderBy(c => c.NumeroCuota).ToList()
        }).ToList();

        return Result<IReadOnlyList<PrestamoDto>>.Success(dtos);
    }
}
