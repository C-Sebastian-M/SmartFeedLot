using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Compradores.Commands.EliminarComprador;

public sealed class EliminarCompradorCommandHandler
    : IRequestHandler<EliminarCompradorCommand, Result>
{
    private readonly ICompradorRepository _compradorRepository;

    public EliminarCompradorCommandHandler(ICompradorRepository compradorRepository)
    {
        _compradorRepository = compradorRepository;
    }

    public async Task<Result> Handle(EliminarCompradorCommand request, CancellationToken ct)
    {
        var eliminado = await _compradorRepository.EliminarAsync(request.Id, ct);
        if (!eliminado)
            return Result.NotFound($"Comprador {request.Id} no encontrado.");
        return Result.Success();
    }
}
