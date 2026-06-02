using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.RegistrarCorteCania;

public sealed record RegistrarCorteCaniaCommand(
    Guid CultivoCaniaId, DateOnly Fecha, int NCalles, decimal Horas,
    int BolsasSilo, decimal Melaza, decimal CostoJornal, string Moneda) : ICommand<Guid>;
public sealed class RegistrarCorteCaniaCommandValidator : AbstractValidator<RegistrarCorteCaniaCommand>
{
    public RegistrarCorteCaniaCommandValidator()
    {
        RuleFor(x => x.CultivoCaniaId).NotEmpty();
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.NCalles).GreaterThan(0);
        RuleFor(x => x.Horas).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BolsasSilo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Melaza).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostoJornal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}

public sealed class RegistrarCorteCaniaCommandHandler : IRequestHandler<RegistrarCorteCaniaCommand, Result<Guid>>
{
    private readonly ICultivoCaniaRepository _repo;
    public RegistrarCorteCaniaCommandHandler(ICultivoCaniaRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(RegistrarCorteCaniaCommand request, CancellationToken ct)
    {
        var cultivo = await _repo.ObtenerPorIdSinTrackingAsync(request.CultivoCaniaId, ct);
        if (cultivo is null) return Result<Guid>.NotFound($"Cultivo de caña {request.CultivoCaniaId} no encontrado.");

        var corte = cultivo.RegistrarCorte(request.Fecha, request.NCalles, request.Horas,
            request.BolsasSilo, request.Melaza, request.CostoJornal, request.Moneda);
        _repo.AgregarCorte(corte);
        return Result<Guid>.Success(corte.Id);
    }
}
