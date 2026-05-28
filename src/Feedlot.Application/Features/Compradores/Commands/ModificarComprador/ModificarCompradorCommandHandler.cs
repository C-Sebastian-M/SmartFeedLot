using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Compradores.Commands.ModificarComprador;

public sealed class ModificarCompradorCommandHandler
    : IRequestHandler<ModificarCompradorCommand, Result>
{
    private readonly ICompradorRepository _compradorRepository;

    public ModificarCompradorCommandHandler(ICompradorRepository compradorRepository)
    {
        _compradorRepository = compradorRepository;
    }

    public async Task<Result> Handle(ModificarCompradorCommand request, CancellationToken ct)
    {
        var comprador = await _compradorRepository.ObtenerPorIdAsync(request.Id, ct);
        if (comprador is null)
            return Result.NotFound($"Comprador {request.Id} no encontrado.");

        var existe = await _compradorRepository.ExisteConNombreAsync(request.Nombre, request.Id, ct);
        if (existe)
            return Result.Conflict($"Ya existe otro comprador con el nombre '{request.Nombre}'.");

        comprador.Modificar(request.Nombre, request.Contacto, request.Telefono, request.Email);
        _compradorRepository.Actualizar(comprador);
        return Result.Success();
    }
}
