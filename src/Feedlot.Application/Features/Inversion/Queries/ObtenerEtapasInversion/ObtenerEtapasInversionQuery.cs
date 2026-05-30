using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Inversion.Queries.ObtenerEtapasInversion;

public sealed record ObtenerEtapasInversionQuery : IRequest<Result<IReadOnlyList<EtapaInversionDto>>>;

public sealed class ItemInversionDto
{
    public Guid Id { get; init; }
    public Guid EtapaId { get; init; }
    public string Producto { get; init; } = null!;
    public decimal Monto { get; init; }
    public string Moneda { get; init; } = null!;
    public string? Observacion { get; init; }
    public string Estado { get; init; } = null!;
    public decimal PorcentajeAvance { get; init; }
}

public sealed class EtapaInversionDto
{
    public Guid Id { get; init; }
    public int Numero { get; init; }
    public string Nombre { get; init; } = null!;
    public decimal TotalRealizadoMonto { get; init; }
    public decimal TotalPendienteMonto { get; init; }
    public string Moneda { get; init; } = null!;
    public List<ItemInversionDto> Items { get; init; } = new();
}

public sealed class ObtenerEtapasInversionQueryHandler
    : IRequestHandler<ObtenerEtapasInversionQuery, Result<IReadOnlyList<EtapaInversionDto>>>
{
    private readonly IEtapaInversionRepository _etapaRepo;

    public ObtenerEtapasInversionQueryHandler(IEtapaInversionRepository etapaRepo)
    {
        _etapaRepo = etapaRepo;
    }

    public async Task<Result<IReadOnlyList<EtapaInversionDto>>> Handle(
        ObtenerEtapasInversionQuery request,
        CancellationToken ct)
    {
        var etapas = await _etapaRepo.ObtenerTodosAsync(ct);

        var dtos = etapas.Select(e => new EtapaInversionDto
        {
            Id = e.Id,
            Numero = e.Numero,
            Nombre = e.Nombre,
            Moneda = e.Moneda,
            TotalRealizadoMonto = e.TotalRealizadoMonto,
            TotalPendienteMonto = e.TotalPendienteMonto,
            Items = e.Items.Select(i => new ItemInversionDto
            {
                Id = i.Id,
                EtapaId = i.EtapaInversionId,
                Producto = i.Producto,
                Monto = i.Costo.Monto,
                Moneda = i.Costo.Moneda,
                Observacion = i.Observacion,
                Estado = i.Estado.ToString(),
                PorcentajeAvance = i.PorcentajeAvance,
            }).ToList()
        }).ToList();

        return Result<IReadOnlyList<EtapaInversionDto>>.Success(dtos);
    }
}
