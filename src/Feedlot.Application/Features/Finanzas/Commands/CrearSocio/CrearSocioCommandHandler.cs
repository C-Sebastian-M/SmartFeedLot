using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearSocio;

public sealed class CrearSocioCommandHandler
    : IRequestHandler<CrearSocioCommand, Result<Guid>>
{
    private readonly ISocioRepository _socioRepo;

    public CrearSocioCommandHandler(
        ISocioRepository socioRepo)
    {
        _socioRepo = socioRepo;
    }

    public async Task<Result<Guid>> Handle(
        CrearSocioCommand request,
        CancellationToken ct)
    {
        var socio = Socio.Crear(request.Nombre, request.Participacion);

        await _socioRepo.AgregarAsync(socio, ct);

        return Result<Guid>.Success(socio.Id);
    }
}
