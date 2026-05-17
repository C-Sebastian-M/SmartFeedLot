using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Nutricion.Commands.CrearRacion;

public sealed class CrearRacionCommandHandler
    : IRequestHandler<CrearRacionCommand, Result<Guid>>
{
    private readonly IRacionRepository _racionRepository;

    public CrearRacionCommandHandler(IRacionRepository racionRepository)
    {
        _racionRepository = racionRepository;
    }

    public async Task<Result<Guid>> Handle(CrearRacionCommand request, CancellationToken ct)
    {
        var nombreExiste = await _racionRepository
            .ExisteNombreAsync(request.Nombre, ct);

        if (nombreExiste)
            return Result<Guid>.Conflict(
                $"Ya existe una ración con el nombre '{request.Nombre}'.");

        var costoKg = Dinero.Crear(request.CostoKg, request.Moneda);

        var racion = Racion.Crear(
            request.Nombre,
            costoKg,
            request.ProteinaPct,
            request.EnergiaMcal);

        await _racionRepository.AgregarAsync(racion, ct);

        return Result<Guid>.Success(racion.Id);
    }
}
