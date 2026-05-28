using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Compradores.Commands.CrearComprador;

public sealed class CrearCompradorCommandHandler
    : IRequestHandler<CrearCompradorCommand, Result<Guid>>
{
    private readonly ICompradorRepository _compradorRepository;

    public CrearCompradorCommandHandler(ICompradorRepository compradorRepository)
    {
        _compradorRepository = compradorRepository;
    }

    public async Task<Result<Guid>> Handle(CrearCompradorCommand request, CancellationToken ct)
    {
        var existe = await _compradorRepository.ExisteConNombreAsync(request.Nombre, null, ct);
        if (existe)
            return Result<Guid>.Conflict($"Ya existe un comprador con el nombre '{request.Nombre}'.");

        var comprador = Comprador.Crear(request.Nombre, request.Contacto, request.Telefono, request.Email);
        await _compradorRepository.AgregarAsync(comprador, ct);
        return Result<Guid>.Success(comprador.Id);
    }
}
