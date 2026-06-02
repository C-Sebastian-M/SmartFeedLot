using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearLoteSilo;

public sealed record CrearLoteSiloCommand(
    DateOnly FechaProduccion, int Bolsas, decimal CostoUnitario,
    string Moneda, string? Observacion, Guid? CorteCaniaId = null) : ICommand<Guid>;
public sealed class CrearLoteSiloCommandValidator : AbstractValidator<CrearLoteSiloCommand>
{
    public CrearLoteSiloCommandValidator()
    {
        RuleFor(x => x.FechaProduccion).NotEmpty();
        RuleFor(x => x.Bolsas).GreaterThan(0);
        RuleFor(x => x.CostoUnitario).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
        RuleFor(x => x.Observacion).MaximumLength(500).When(x => x.Observacion != null);
    }
}

public sealed class CrearLoteSiloCommandHandler : IRequestHandler<CrearLoteSiloCommand, Result<Guid>>
{
    private readonly ILoteSiloRepository _repo;
    public CrearLoteSiloCommandHandler(ILoteSiloRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(CrearLoteSiloCommand request, CancellationToken ct)
    {
        var costoUnitario = Dinero.Crear(request.CostoUnitario, request.Moneda);
        var lote = LoteSilo.Crear(request.FechaProduccion, request.Bolsas, costoUnitario, request.Observacion, request.CorteCaniaId);
        await _repo.AgregarAsync(lote, ct);
        return Result<Guid>.Success(lote.Id);
    }
}
