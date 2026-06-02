using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearPotrero;

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
