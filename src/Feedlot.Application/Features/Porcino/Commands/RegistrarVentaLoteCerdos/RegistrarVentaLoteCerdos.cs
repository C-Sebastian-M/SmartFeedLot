using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.RegistrarVentaLoteCerdos;

public sealed record RegistrarVentaLoteCerdosCommand(
    Guid LoteId,
    DateOnly FechaVenta,
    decimal PrecioVentaKg,
    string Moneda) : ICommand;
public sealed class RegistrarVentaLoteCerdosCommandValidator : AbstractValidator<RegistrarVentaLoteCerdosCommand>
{
    public RegistrarVentaLoteCerdosCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.FechaVenta).NotEmpty();
        RuleFor(x => x.PrecioVentaKg).GreaterThan(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}

public sealed class RegistrarVentaLoteCerdosCommandHandler : IRequestHandler<RegistrarVentaLoteCerdosCommand, Result>
{
    private readonly ILoteCerdosRepository _repo;
    public RegistrarVentaLoteCerdosCommandHandler(ILoteCerdosRepository repo) { _repo = repo; }

    public async Task<Result> Handle(RegistrarVentaLoteCerdosCommand request, CancellationToken ct)
    {
        var lote = await _repo.ObtenerPorIdAsync(request.LoteId, ct);
        if (lote is null)
            return Result.NotFound($"Lote de cerdos con Id {request.LoteId} no encontrado.");

        var precio = Dinero.Crear(request.PrecioVentaKg, request.Moneda);
        lote.RegistrarVenta(request.FechaVenta, precio);
        return Result.Success();
    }
}
