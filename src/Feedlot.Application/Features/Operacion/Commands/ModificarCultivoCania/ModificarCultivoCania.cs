using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.ModificarCultivoCania;

public sealed record ModificarCultivoCaniaCommand(Guid CultivoCaniaId, string Nombre, int CallesTotales) : ICommand;

public sealed class ModificarCultivoCaniaCommandValidator : AbstractValidator<ModificarCultivoCaniaCommand>
{
    public ModificarCultivoCaniaCommandValidator()
    {
        RuleFor(x => x.CultivoCaniaId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CallesTotales).GreaterThan(0);
    }
}

public sealed class ModificarCultivoCaniaCommandHandler : IRequestHandler<ModificarCultivoCaniaCommand, Result>
{
    private readonly ICultivoCaniaRepository _repo;

    public ModificarCultivoCaniaCommandHandler(ICultivoCaniaRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(ModificarCultivoCaniaCommand request, CancellationToken ct)
    {
        var cultivo = await _repo.ObtenerPorIdAsync(request.CultivoCaniaId, ct);
        if (cultivo is null)
            return Result.NotFound($"No se encontró el cultivo {request.CultivoCaniaId}.");

        cultivo.Modificar(request.Nombre, request.CallesTotales);
        _repo.Actualizar(cultivo);
        return Result.Success();
    }
}
