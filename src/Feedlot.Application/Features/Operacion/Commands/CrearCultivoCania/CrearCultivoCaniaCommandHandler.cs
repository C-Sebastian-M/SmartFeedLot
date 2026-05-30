using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Operacion.Commands.CrearCultivoCania;

public sealed class CrearCultivoCaniaCommandHandler : IRequestHandler<CrearCultivoCaniaCommand, Result<Guid>>
{
    private readonly ICultivoCaniaRepository _repo;
    private readonly IUnitOfWork _uow;
    public CrearCultivoCaniaCommandHandler(ICultivoCaniaRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<Result<Guid>> Handle(CrearCultivoCaniaCommand request, CancellationToken ct)
    {
        var cultivo = CultivoCania.Crear(request.Nombre, request.CallesTotales);
        await _repo.AgregarAsync(cultivo, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(cultivo.Id);
    }
}
