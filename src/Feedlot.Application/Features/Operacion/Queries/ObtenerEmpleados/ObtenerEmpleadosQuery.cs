using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Queries.ObtenerEmpleados;

public sealed record ObtenerEmpleadosQuery : IRequest<Result<IReadOnlyList<EmpleadoDto>>>;

public sealed class ActividadDto
{
    public Guid Id { get; init; }
    public string Tipo { get; init; } = null!;
    public DateOnly Fecha { get; init; }
    public decimal CostoMonto { get; init; }
    public string CostoMoneda { get; init; } = null!;
}

public sealed class EmpleadoDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = null!;
    public decimal PagoMensualMonto { get; init; }
    public string PagoMensualMoneda { get; init; } = null!;
    public List<ActividadDto> Actividades { get; init; } = new();
}

public sealed class ObtenerEmpleadosQueryHandler : IRequestHandler<ObtenerEmpleadosQuery, Result<IReadOnlyList<EmpleadoDto>>>
{
    private readonly IEmpleadoRepository _repo;
    public ObtenerEmpleadosQueryHandler(IEmpleadoRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<EmpleadoDto>>> Handle(ObtenerEmpleadosQuery request, CancellationToken ct)
    {
        var empleados = await _repo.ObtenerTodosAsync(ct);
        var dtos = empleados.Select(e => new EmpleadoDto
        {
            Id = e.Id,
            Nombre = e.Nombre,
            PagoMensualMonto = e.PagoMensual.Monto,
            PagoMensualMoneda = e.PagoMensual.Moneda,
            Actividades = e.Actividades.Select(a => new ActividadDto
            {
                Id = a.Id, Tipo = a.Tipo, Fecha = a.Fecha,
                CostoMonto = a.Costo.Monto, CostoMoneda = a.Costo.Moneda,
            }).ToList()
        }).ToList();
        return Result<IReadOnlyList<EmpleadoDto>>.Success(dtos);
    }
}
