using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Inversion.Commands.CrearEtapaInversion;

public sealed record CrearEtapaInversionCommand(
    int Numero,
    string Nombre
) : ICommand<Guid>;
public sealed class CrearEtapaInversionCommandValidator : AbstractValidator<CrearEtapaInversionCommand>
{
    public CrearEtapaInversionCommandValidator()
    {
        RuleFor(x => x.Numero).InclusiveBetween(1, 5);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}

public sealed class CrearEtapaInversionCommandHandler
    : IRequestHandler<CrearEtapaInversionCommand, Result<Guid>>
{
    private readonly IEtapaInversionRepository _etapaRepo;

    public CrearEtapaInversionCommandHandler(
        IEtapaInversionRepository etapaRepo)
    {
        _etapaRepo = etapaRepo;
    }

    public async Task<Result<Guid>> Handle(
        CrearEtapaInversionCommand request,
        CancellationToken ct)
    {
        var etapa = EtapaInversion.Crear(request.Numero, request.Nombre);

        await _etapaRepo.AgregarAsync(etapa, ct);

        return Result<Guid>.Success(etapa.Id);
    }
}
