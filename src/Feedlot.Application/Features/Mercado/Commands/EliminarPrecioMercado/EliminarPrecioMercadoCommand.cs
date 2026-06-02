using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Commands.EliminarPrecioMercado;

public sealed record EliminarPrecioMercadoCommand(Guid Id) : ICommand;

public sealed class EliminarPrecioMercadoCommandHandler : IRequestHandler<EliminarPrecioMercadoCommand, Result>
{
    private readonly IPrecioMercadoRepository _repo;
    private readonly IUnitOfWork _uow;

    public EliminarPrecioMercadoCommandHandler(IPrecioMercadoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(EliminarPrecioMercadoCommand request, CancellationToken ct)
    {
        var precio = await _repo.ObtenerPorIdAsync(request.Id, ct);
        if (precio is null)
            return Result.NotFound($"No se encontró el precio de mercado {request.Id}.");

        _repo.Eliminar(precio);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
