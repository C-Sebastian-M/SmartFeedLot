using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Commands.EliminarPrecioMercado;

public sealed record EliminarPrecioMercadoCommand(Guid Id) : ICommand;

public sealed class EliminarPrecioMercadoCommandHandler : IRequestHandler<EliminarPrecioMercadoCommand, Result>
{
    private readonly IPrecioMercadoRepository _repo;

    public EliminarPrecioMercadoCommandHandler(IPrecioMercadoRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(EliminarPrecioMercadoCommand request, CancellationToken ct)
    {
        var precio = await _repo.ObtenerPorIdAsync(request.Id, ct);
        if (precio is null)
            return Result.NotFound($"No se encontró el precio de mercado {request.Id}.");

        _repo.Eliminar(precio);
        return Result.Success();
    }
}
