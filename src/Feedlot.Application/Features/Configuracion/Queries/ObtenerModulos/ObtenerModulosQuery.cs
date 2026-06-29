using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Configuracion.Queries.ObtenerModulos;

/// <summary>
/// Lista todos los módulos del sistema con su estado (activo/inactivo).
/// La usa el frontend para decidir qué entradas mostrar en el menú y
/// la pantalla de configuración del Admin.
/// </summary>
public sealed record ObtenerModulosQuery : IRequest<Result<IReadOnlyList<ModuloDto>>>;

public sealed record ModuloDto(
    Guid Id,
    string Clave,
    string Nombre,
    bool Activo,
    int Orden);

public sealed class ObtenerModulosQueryHandler
    : IRequestHandler<ObtenerModulosQuery, Result<IReadOnlyList<ModuloDto>>>
{
    private readonly IModuloSistemaRepository _repo;
    public ObtenerModulosQueryHandler(IModuloSistemaRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<ModuloDto>>> Handle(
        ObtenerModulosQuery request, CancellationToken ct)
    {
        var modulos = await _repo.ObtenerTodosAsync(ct);
        var dtos = modulos
            .Select(m => new ModuloDto(m.Id, m.Clave, m.Nombre, m.Activo, m.Orden))
            .ToList()
            .AsReadOnly();
        return Result<IReadOnlyList<ModuloDto>>.Success(dtos);
    }
}
