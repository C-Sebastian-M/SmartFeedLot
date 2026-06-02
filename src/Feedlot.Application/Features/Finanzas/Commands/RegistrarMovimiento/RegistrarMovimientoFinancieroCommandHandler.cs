using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.RegistrarMovimiento;

public sealed class RegistrarMovimientoFinancieroCommandHandler
    : IRequestHandler<RegistrarMovimientoFinancieroCommand, Result<Guid>>
{
    private readonly IMovimientoFinancieroRepository _movimientoRepo;
    private readonly ICategoriaGastoRepository _categoriaRepo;
    private readonly ISocioRepository _socioRepo;

    public RegistrarMovimientoFinancieroCommandHandler(
        IMovimientoFinancieroRepository movimientoRepo,
        ICategoriaGastoRepository categoriaRepo,
        ISocioRepository socioRepo)
    {
        _movimientoRepo = movimientoRepo;
        _categoriaRepo = categoriaRepo;
        _socioRepo = socioRepo;
    }

    public async Task<Result<Guid>> Handle(
        RegistrarMovimientoFinancieroCommand request,
        CancellationToken ct)
    {
        var categoria = await _categoriaRepo.ObtenerPorIdAsync(request.CategoriaGastoId, ct);
        if (categoria is null)
            return Result<Guid>.NotFound(
                $"No se encontró la categoría de gasto con ID '{request.CategoriaGastoId}'.");

        if (request.SocioId.HasValue)
        {
            var socio = await _socioRepo.ObtenerPorIdAsync(request.SocioId.Value, ct);
            if (socio is null)
                return Result<Guid>.NotFound(
                    $"No se encontró el socio con ID '{request.SocioId.Value}'.");
        }

        if (!Enum.TryParse<OrigenFinanciero>(request.Origen, ignoreCase: true, out var origen))
            return Result<Guid>.Failure("Origen financiero inválido. Valores: Bovino, Porcino, Agricola, General.");

        var monto = Dinero.Crear(request.Monto, request.Moneda);

        var movimiento = MovimientoFinanciero.Registrar(
            request.Fecha,
            request.PeriodoAnio,
            request.PeriodoMes,
            request.CategoriaGastoId,
            monto,
            origen,
            request.Descripcion,
            request.SocioId,
            request.RegistradoPorId);

        await _movimientoRepo.AgregarAsync(movimiento, ct);

        return Result<Guid>.Success(movimiento.Id);
    }
}
