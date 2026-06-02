using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.IngresarAnimalPotrero;

public sealed record IngresarAnimalPotreroCommand(Guid PotreroId, Guid AnimalId, DateOnly FechaEntrada) : ICommand<Guid>;
public sealed class IngresarAnimalPotreroCommandValidator : AbstractValidator<IngresarAnimalPotreroCommand>
{
    public IngresarAnimalPotreroCommandValidator()
    {
        RuleFor(x => x.PotreroId).NotEmpty();
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.FechaEntrada).NotEmpty();
    }
}

public sealed class IngresarAnimalPotreroCommandHandler : IRequestHandler<IngresarAnimalPotreroCommand, Result<Guid>>
{
    private readonly IPotreroRepository _repo;
    public IngresarAnimalPotreroCommandHandler(IPotreroRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(IngresarAnimalPotreroCommand request, CancellationToken ct)
    {
        // AsNoTracking: el Potrero se carga solo para validar (capacidad, duplicados).
        // Así el nuevo EstanciaAnimal que devuelve IngresarAnimal() no forma parte
        // del grafo tracked y Add() lo registra limpio como EntityState.Added → INSERT.
        var potrero = await _repo.ObtenerPorIdSinTrackingAsync(request.PotreroId, ct);
        if (potrero is null) return Result<Guid>.NotFound($"Potrero {request.PotreroId} no encontrado.");

        var estancia = potrero.IngresarAnimal(request.AnimalId, request.FechaEntrada);
        _repo.AgregarEstancia(estancia);
        return Result<Guid>.Success(estancia.Id);
    }
}
