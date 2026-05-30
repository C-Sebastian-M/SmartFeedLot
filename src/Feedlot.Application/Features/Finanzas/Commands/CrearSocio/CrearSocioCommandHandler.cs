using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearSocio;

public sealed class CrearSocioCommandHandler
    : IRequestHandler<CrearSocioCommand, Result<Guid>>
{
    private readonly ISocioRepository _socioRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CrearSocioCommandHandler(
        ISocioRepository socioRepo,
        IUnitOfWork unitOfWork)
    {
        _socioRepo = socioRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CrearSocioCommand request,
        CancellationToken ct)
    {
        var socio = Socio.Crear(request.Nombre, request.Participacion);

        await _socioRepo.AgregarAsync(socio, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(socio.Id);
    }
}
