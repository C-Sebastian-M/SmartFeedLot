using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.RetirarAnimalPotrero;

public sealed record RetirarAnimalPotreroCommand(Guid PotreroId, Guid EstanciaId, DateOnly FechaSalida) : ICommand;
public sealed class RetirarAnimalPotreroCommandValidator : AbstractValidator<RetirarAnimalPotreroCommand>
{
    public RetirarAnimalPotreroCommandValidator()
    {
        RuleFor(x => x.PotreroId).NotEmpty();
        RuleFor(x => x.EstanciaId).NotEmpty();
        RuleFor(x => x.FechaSalida)
            .NotEmpty()
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de salida no puede ser futura.");
    }
}

public sealed class RetirarAnimalPotreroCommandHandler : IRequestHandler<RetirarAnimalPotreroCommand, Result>
{
    private readonly IPotreroRepository _repo;

    public RetirarAnimalPotreroCommandHandler(IPotreroRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(RetirarAnimalPotreroCommand request, CancellationToken ct)
    {
        var potrero = await _repo.ObtenerPorIdAsync(request.PotreroId, ct);
        if (potrero is null)
            return Result.NotFound($"No se encontró el potrero {request.PotreroId}.");

        try
        {
            potrero.RetirarAnimal(request.EstanciaId, request.FechaSalida);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
