using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.CrearLoteCerdos;

public sealed record CrearLoteCerdosCommand(
    string Codigo,
    DateOnly FechaInicio,
    int NAnimales,
    decimal PesoPromedioKg,
    string Ciclo,
    Guid? CamadaId = null,
    decimal? PrecioVentaKg = null,
    string? Moneda = null) : ICommand<Guid>;
public sealed class CrearLoteCerdosCommandValidator : AbstractValidator<CrearLoteCerdosCommand>
{
    public CrearLoteCerdosCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FechaInicio).NotEmpty();
        RuleFor(x => x.NAnimales).GreaterThan(0);
        RuleFor(x => x.PesoPromedioKg).GreaterThan(0);
        RuleFor(x => x.Ciclo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Moneda).Length(3).When(x => x.Moneda != null);
    }
}

public sealed class CrearLoteCerdosCommandHandler : IRequestHandler<CrearLoteCerdosCommand, Result<Guid>>
{
    private readonly ILoteCerdosRepository _repo;
    public CrearLoteCerdosCommandHandler(ILoteCerdosRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(CrearLoteCerdosCommand request, CancellationToken ct)
    {
        Dinero? precioVentaKg = null;
        if (request.PrecioVentaKg.HasValue)
            precioVentaKg = Dinero.Crear(request.PrecioVentaKg.Value, request.Moneda ?? "COP");

        var lote = LoteCerdos.Crear(request.Codigo, request.FechaInicio, request.NAnimales,
            request.PesoPromedioKg, request.Ciclo, request.CamadaId, precioVentaKg);
        await _repo.AgregarAsync(lote, ct);
        return Result<Guid>.Success(lote.Id);
    }
}
