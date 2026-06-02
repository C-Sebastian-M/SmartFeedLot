using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Inversion.Commands.CrearAporteSocio;

public sealed record CrearAporteSocioCommand(
    Guid SocioId,
    Guid ItemInversionId,
    decimal Monto,
    string Moneda
) : ICommand<Guid>;
public sealed class CrearAporteSocioCommandValidator : AbstractValidator<CrearAporteSocioCommand>
{
    public CrearAporteSocioCommandValidator()
    {
        RuleFor(x => x.SocioId).NotEmpty();
        RuleFor(x => x.ItemInversionId).NotEmpty();
        RuleFor(x => x.Monto).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Moneda).Length(3);
    }
}

public sealed class CrearAporteSocioCommandHandler
    : IRequestHandler<CrearAporteSocioCommand, Result<Guid>>
{
    private readonly IAporteSocioRepository _aporteRepo;

    public CrearAporteSocioCommandHandler(
        IAporteSocioRepository aporteRepo)
    {
        _aporteRepo = aporteRepo;
    }

    public async Task<Result<Guid>> Handle(
        CrearAporteSocioCommand request,
        CancellationToken ct)
    {
        var monto = Dinero.Crear(request.Monto, request.Moneda);

        var aporte = AporteSocio.Crear(request.SocioId, request.ItemInversionId, monto);

        await _aporteRepo.AgregarAsync(aporte, ct);

        return Result<Guid>.Success(aporte.Id);
    }
}
