using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.RegistrarCamada;

public sealed class RegistrarCamadaCommandHandler : IRequestHandler<RegistrarCamadaCommand, Result<Guid>>
{
    private readonly IMarranaRepository _repo;
    private readonly IUnitOfWork _uow;
    public RegistrarCamadaCommandHandler(IMarranaRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result<Guid>> Handle(RegistrarCamadaCommand request, CancellationToken ct)
    {
        var marrana = await _repo.ObtenerPorIdSinTrackingAsync(request.MarranaId, ct);
        if (marrana is null)
            return Result<Guid>.NotFound($"Marrana con Id {request.MarranaId} no encontrada.");

        var camada = marrana.RegistrarCamada(request.FechaNacimiento, request.NLechones);
        _repo.AgregarCamada(camada);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(camada.Id);
    }
}
