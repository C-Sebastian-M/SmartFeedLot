using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.CrearMarrana;

public sealed class CrearMarranaCommandHandler : IRequestHandler<CrearMarranaCommand, Result<Guid>>
{
    private readonly IMarranaRepository _repo;
    private readonly IUnitOfWork _uow;
    public CrearMarranaCommandHandler(IMarranaRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result<Guid>> Handle(CrearMarranaCommand request, CancellationToken ct)
    {
        var costo = Dinero.Crear(request.Costo, request.Moneda);
        var marrana = Marrana.Crear(request.Identificacion, request.FechaCompra, costo);
        await _repo.AgregarAsync(marrana, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(marrana.Id);
    }
}
