using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.GuardarPresupuesto;

/// <summary>
/// Crea o actualiza la línea de presupuesto de una categoría para un período.
/// Upsert: si ya existe, lo modifica; si no, lo crea.
/// </summary>
public sealed record GuardarPresupuestoCommand(
    int PeriodoAnio,
    int PeriodoMes,
    Guid CategoriaGastoId,
    decimal Monto,
    string Moneda,
    string? Descripcion
) : IRequest<Result<Guid>>;

public sealed class GuardarPresupuestoCommandHandler
    : IRequestHandler<GuardarPresupuestoCommand, Result<Guid>>
{
    private readonly IPresupuestoRepository _presupuestoRepo;
    private readonly ICategoriaGastoRepository _categoriaRepo;
    private readonly IUnitOfWork _uow;

    public GuardarPresupuestoCommandHandler(
        IPresupuestoRepository presupuestoRepo,
        ICategoriaGastoRepository categoriaRepo,
        IUnitOfWork uow)
    {
        _presupuestoRepo = presupuestoRepo;
        _categoriaRepo = categoriaRepo;
        _uow = uow;
    }

    public async Task<Result<Guid>> Handle(
        GuardarPresupuestoCommand request, CancellationToken ct)
    {
        var categoria = await _categoriaRepo.ObtenerPorIdAsync(request.CategoriaGastoId, ct);
        if (categoria is null)
            return Result<Guid>.NotFound($"No se encontró la categoría {request.CategoriaGastoId}.");

        var dinero = Dinero.Crear(request.Monto, request.Moneda);

        var existente = await _presupuestoRepo.ObtenerPorPeriodoCategoriaAsync(
            request.PeriodoAnio, request.PeriodoMes, request.CategoriaGastoId, ct);

        if (existente is not null)
        {
            existente.Modificar(dinero, request.Descripcion);
            _presupuestoRepo.Actualizar(existente);
            await _uow.SaveChangesAsync(ct);
            return Result<Guid>.Success(existente.Id);
        }

        var nuevo = Domain.Entities.Presupuesto.Crear(
            request.PeriodoAnio,
            request.PeriodoMes,
            request.CategoriaGastoId,
            dinero,
            request.Descripcion);

        await _presupuestoRepo.AgregarAsync(nuevo, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(nuevo.Id);
    }
}
