using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Queries.ObtenerCategoriasGasto;

public sealed record ObtenerCategoriasGastoQuery : IRequest<Result<IReadOnlyList<CategoriaGastoDto>>>;

public sealed class CategoriaGastoDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = null!;
    public string Tipo { get; init; } = null!;
}

public sealed class ObtenerCategoriasGastoQueryHandler
    : IRequestHandler<ObtenerCategoriasGastoQuery, Result<IReadOnlyList<CategoriaGastoDto>>>
{
    private readonly ICategoriaGastoRepository _categoriaRepo;

    public ObtenerCategoriasGastoQueryHandler(ICategoriaGastoRepository categoriaRepo)
    {
        _categoriaRepo = categoriaRepo;
    }

    public async Task<Result<IReadOnlyList<CategoriaGastoDto>>> Handle(
        ObtenerCategoriasGastoQuery request,
        CancellationToken ct)
    {
        var categorias = await _categoriaRepo.ObtenerTodosAsync(ct);
        var dtos = categorias.Select(c => new CategoriaGastoDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Tipo = c.Tipo.ToString()
        }).ToList();

        return Result<IReadOnlyList<CategoriaGastoDto>>.Success(dtos);
    }
}
