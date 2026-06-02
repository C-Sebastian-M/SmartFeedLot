using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Commands.ActualizarPrecioMercado;

public sealed record ActualizarPrecioMercadoCommand(
    Guid Id,
    DateOnly Fecha,
    string Especie,
    string Tipo,
    decimal PrecioPorKg,
    string Fuente
) : ICommand;

public sealed class ActualizarPrecioMercadoCommandValidator : AbstractValidator<ActualizarPrecioMercadoCommand>
{
    public ActualizarPrecioMercadoCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Fecha).NotEmpty().WithMessage("La fecha es requerida.");
        RuleFor(x => x.Especie).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Tipo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PrecioPorKg).GreaterThan(0).WithMessage("El precio por kg debe ser mayor a cero.");
        RuleFor(x => x.Fuente).NotEmpty().MaximumLength(200);
    }
}

public sealed class ActualizarPrecioMercadoCommandHandler : IRequestHandler<ActualizarPrecioMercadoCommand, Result>
{
    private readonly IPrecioMercadoRepository _repo;
    private readonly IUnitOfWork _uow;

    public ActualizarPrecioMercadoCommandHandler(IPrecioMercadoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result> Handle(ActualizarPrecioMercadoCommand request, CancellationToken ct)
    {
        var precio = await _repo.ObtenerPorIdAsync(request.Id, ct);
        if (precio is null)
            return Result.NotFound($"No se encontró el precio de mercado {request.Id}.");

        precio.Modificar(request.Fecha, request.Especie, request.Tipo, request.PrecioPorKg, request.Fuente);
        _repo.Actualizar(precio);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
