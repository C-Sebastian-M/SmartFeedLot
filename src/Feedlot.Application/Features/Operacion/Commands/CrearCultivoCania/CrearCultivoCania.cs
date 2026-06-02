using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearCultivoCania;

public sealed record CrearCultivoCaniaCommand(string Nombre, int CallesTotales) : ICommand<Guid>;
public sealed class CrearCultivoCaniaCommandValidator : AbstractValidator<CrearCultivoCaniaCommand>
{
    public CrearCultivoCaniaCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CallesTotales).GreaterThan(0);
    }
}

public sealed class CrearCultivoCaniaCommandHandler : IRequestHandler<CrearCultivoCaniaCommand, Result<Guid>>
{
    private readonly ICultivoCaniaRepository _repo;
    public CrearCultivoCaniaCommandHandler(ICultivoCaniaRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(CrearCultivoCaniaCommand request, CancellationToken ct)
    {
        var cultivo = CultivoCania.Crear(request.Nombre, request.CallesTotales);
        await _repo.AgregarAsync(cultivo, ct);
        return Result<Guid>.Success(cultivo.Id);
    }
}
