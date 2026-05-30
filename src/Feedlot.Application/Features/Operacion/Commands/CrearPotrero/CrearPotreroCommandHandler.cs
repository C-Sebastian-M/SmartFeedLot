using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearPotrero;

public sealed class CrearPotreroCommandHandler : IRequestHandler<CrearPotreroCommand, Result<Guid>>
{
    private readonly IPotreroRepository _repo;
    private readonly IUnitOfWork _uow;
    public CrearPotreroCommandHandler(IPotreroRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result<Guid>> Handle(CrearPotreroCommand request, CancellationToken ct)
    {
        var potrero = Potrero.Crear(request.Nombre, request.Capacidad);
        await _repo.AgregarAsync(potrero, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(potrero.Id);
    }
}
