using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Configuracion.Commands.CambiarEstadoModulo;

/// <summary>
/// Activa o desactiva un módulo del sistema. Solo el Admin debería invocarlo
/// (la restricción se aplica en el controller con [Authorize(Roles="Admin")]).
/// </summary>
public sealed record CambiarEstadoModuloCommand(string Clave, bool Activo) : ICommand;

public sealed class CambiarEstadoModuloCommandHandler
    : IRequestHandler<CambiarEstadoModuloCommand, Result>
{
    private readonly IModuloSistemaRepository _repo;
    private readonly IUnitOfWork _uow;

    public CambiarEstadoModuloCommandHandler(IModuloSistemaRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(CambiarEstadoModuloCommand request, CancellationToken ct)
    {
        var modulo = await _repo.ObtenerPorClaveAsync(request.Clave, ct);
        if (modulo is null)
            return Result.NotFound($"No existe el módulo '{request.Clave}'.");

        modulo.EstablecerActivo(request.Activo);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
