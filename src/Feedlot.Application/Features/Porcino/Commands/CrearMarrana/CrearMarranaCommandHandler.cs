using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.CrearMarrana;

public sealed class CrearMarranaCommandHandler : IRequestHandler<CrearMarranaCommand, Result<Guid>>
{
    private readonly IMarranaRepository _repo;
    public CrearMarranaCommandHandler(IMarranaRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(CrearMarranaCommand request, CancellationToken ct)
    {
        var costo = Dinero.Crear(request.Costo, request.Moneda);
        var marrana = Marrana.Crear(request.Identificacion, request.FechaCompra, costo);
        await _repo.AgregarAsync(marrana, ct);
        return Result<Guid>.Success(marrana.Id);
    }
}
