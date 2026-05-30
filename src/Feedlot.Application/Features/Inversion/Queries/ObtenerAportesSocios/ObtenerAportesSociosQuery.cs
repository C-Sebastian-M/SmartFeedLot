using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Inversion.Queries.ObtenerAportesSocios;

public sealed record ObtenerAportesSociosQuery(Guid? SocioId = null, Guid? ItemInversionId = null)
    : IRequest<Result<IReadOnlyList<AporteSocioDto>>>;

public sealed class AporteSocioDto
{
    public Guid Id { get; init; }
    public Guid SocioId { get; init; }
    public string SocioNombre { get; init; } = null!;
    public Guid ItemInversionId { get; init; }
    public string ItemProducto { get; init; } = null!;
    public decimal Monto { get; init; }
    public string Moneda { get; init; } = null!;
}

public sealed class ObtenerAportesSociosQueryHandler
    : IRequestHandler<ObtenerAportesSociosQuery, Result<IReadOnlyList<AporteSocioDto>>>
{
    private readonly IAporteSocioRepository _aporteRepo;

    public ObtenerAportesSociosQueryHandler(IAporteSocioRepository aporteRepo)
    {
        _aporteRepo = aporteRepo;
    }

    public async Task<Result<IReadOnlyList<AporteSocioDto>>> Handle(
        ObtenerAportesSociosQuery request,
        CancellationToken ct)
    {
        IReadOnlyList<Domain.Entities.AporteSocio> aportes;

        if (request.ItemInversionId.HasValue)
            aportes = await _aporteRepo.ObtenerPorItemAsync(request.ItemInversionId.Value, ct);
        else if (request.SocioId.HasValue)
            aportes = await _aporteRepo.ObtenerPorSocioAsync(request.SocioId.Value, ct);
        else
            return Result<IReadOnlyList<AporteSocioDto>>.Success(new List<AporteSocioDto>());

        var dtos = aportes.Select(a => new AporteSocioDto
        {
            Id = a.Id,
            SocioId = a.SocioId,
            SocioNombre = "",
            ItemInversionId = a.ItemInversionId,
            ItemProducto = "",
            Monto = a.Monto.Monto,
            Moneda = a.Monto.Moneda,
        }).ToList();

        return Result<IReadOnlyList<AporteSocioDto>>.Success(dtos);
    }
}
