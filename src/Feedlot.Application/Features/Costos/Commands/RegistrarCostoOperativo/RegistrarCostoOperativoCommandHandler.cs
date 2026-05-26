using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Costos.Commands.RegistrarCostoOperativo;

public sealed class RegistrarCostoOperativoCommandHandler
    : IRequestHandler<RegistrarCostoOperativoCommand, Result<Guid>>
{
    private readonly ICostoOperativoRepository _costoRepo;
    private readonly ILoteRepository _loteRepo;

    public RegistrarCostoOperativoCommandHandler(
        ICostoOperativoRepository costoRepo,
        ILoteRepository loteRepo)
    {
        _costoRepo = costoRepo;
        _loteRepo = loteRepo;
    }

    public async Task<Result<Guid>> Handle(
        RegistrarCostoOperativoCommand request,
        CancellationToken ct)
    {
        var lote = await _loteRepo.ObtenerPorIdAsync(request.LoteId, ct);
        if (lote is null)
            return Result<Guid>.NotFound(
                $"No se encontró el lote con ID '{request.LoteId}'.");

        var categoria = Enum.Parse<CategoriaCosto>(request.Categoria, ignoreCase: true);
        var monto = Dinero.Crear(request.Monto, request.Moneda);

        var costo = CostoOperativo.Registrar(
            request.LoteId,
            categoria,
            request.Concepto,
            request.Fecha,
            monto,
            request.Observaciones,
            request.RegistradoPorId);

        await _costoRepo.AgregarAsync(costo, ct);
        return Result<Guid>.Success(costo.Id);
    }
}
