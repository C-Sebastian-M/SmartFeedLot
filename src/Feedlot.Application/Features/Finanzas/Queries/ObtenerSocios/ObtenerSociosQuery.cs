using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Queries.ObtenerSocios;

public sealed record ObtenerSociosQuery : IRequest<Result<IReadOnlyList<SocioDto>>>;

public sealed class SocioDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = null!;
    public decimal Participacion { get; init; }
}

public sealed class ObtenerSociosQueryHandler
    : IRequestHandler<ObtenerSociosQuery, Result<IReadOnlyList<SocioDto>>>
{
    private readonly ISocioRepository _socioRepo;

    public ObtenerSociosQueryHandler(ISocioRepository socioRepo)
    {
        _socioRepo = socioRepo;
    }

    public async Task<Result<IReadOnlyList<SocioDto>>> Handle(
        ObtenerSociosQuery request,
        CancellationToken ct)
    {
        var socios = await _socioRepo.ObtenerTodosAsync(ct);
        var dtos = socios.Select(s => new SocioDto
        {
            Id = s.Id,
            Nombre = s.Nombre,
            Participacion = s.Participacion
        }).ToList();

        return Result<IReadOnlyList<SocioDto>>.Success(dtos);
    }
}
