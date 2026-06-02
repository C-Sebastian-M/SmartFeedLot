using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Porcino.Commands.RegistrarCamada;

public sealed class RegistrarCamadaCommandHandler : IRequestHandler<RegistrarCamadaCommand, Result<Guid>>
{
    private readonly IMarranaRepository _repo;
    public RegistrarCamadaCommandHandler(IMarranaRepository repo) { _repo = repo; }

    public async Task<Result<Guid>> Handle(RegistrarCamadaCommand request, CancellationToken ct)
    {
        var marrana = await _repo.ObtenerPorIdSinTrackingAsync(request.MarranaId, ct);
        if (marrana is null)
            return Result<Guid>.NotFound($"Marrana con Id {request.MarranaId} no encontrada.");

        var camada = marrana.RegistrarCamada(request.FechaNacimiento, request.NLechones);
        _repo.AgregarCamada(camada);
        return Result<Guid>.Success(camada.Id);
    }
}
