using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Queries.ObtenerPotreros;

public sealed record ObtenerPotrerosQuery : IRequest<Result<IReadOnlyList<PotreroDto>>>;

public sealed class PotreroDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = null!;
    public int Capacidad { get; init; }
    public int AnimalesActuales { get; init; }
    public List<EstanciaDto> Estancias { get; init; } = new();
}

public sealed class EstanciaDto
{
    public Guid Id { get; init; }
    public Guid AnimalId { get; init; }
    public DateOnly FechaEntrada { get; init; }
    public DateOnly? Salida { get; init; }
}

public sealed class ObtenerPotrerosQueryHandler : IRequestHandler<ObtenerPotrerosQuery, Result<IReadOnlyList<PotreroDto>>>
{
    private readonly IPotreroRepository _repo;
    public ObtenerPotrerosQueryHandler(IPotreroRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<PotreroDto>>> Handle(ObtenerPotrerosQuery request, CancellationToken ct)
    {
        var potreros = await _repo.ObtenerTodosAsync(ct);
        var dtos = potreros.Select(p => new PotreroDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Capacidad = p.Capacidad,
            AnimalesActuales = p.AnimalesActuales,
            Estancias = p.Estancias.Select(e => new EstanciaDto
            {
                Id = e.Id, AnimalId = e.AnimalId, FechaEntrada = e.FechaEntrada, Salida = e.Salida,
            }).ToList()
        }).ToList();
        return Result<IReadOnlyList<PotreroDto>>.Success(dtos);
    }
}
