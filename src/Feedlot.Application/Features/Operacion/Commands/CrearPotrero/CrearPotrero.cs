using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearPotrero;

public sealed record CrearPotreroCommand(string Nombre, int Capacidad) : ICommand<Guid>;
public sealed class CrearPotreroCommandValidator : AbstractValidator<CrearPotreroCommand>
{
    public CrearPotreroCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacidad).GreaterThan(0);
    }
}

public sealed class CrearPotreroCommandHandler : IRequestHandler<CrearPotreroCommand, Result<Guid>>
{
    private readonly IPotreroRepository _repo;
    public CrearPotreroCommandHandler(IPotreroRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(CrearPotreroCommand request, CancellationToken ct)
    {
        var potrero = Potrero.Crear(request.Nombre, request.Capacidad);
        await _repo.AgregarAsync(potrero, ct);
        return Result<Guid>.Success(potrero.Id);
    }
}
