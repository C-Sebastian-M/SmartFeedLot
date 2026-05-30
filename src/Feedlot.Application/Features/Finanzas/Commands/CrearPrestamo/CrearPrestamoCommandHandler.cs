using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearPrestamo;

public sealed class CrearPrestamoCommandHandler
    : IRequestHandler<CrearPrestamoCommand, Result<Guid>>
{
    private readonly IPrestamoRepository _prestamoRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CrearPrestamoCommandHandler(
        IPrestamoRepository prestamoRepo,
        IUnitOfWork unitOfWork)
    {
        _prestamoRepo = prestamoRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CrearPrestamoCommand request,
        CancellationToken ct)
    {
        var capital = Dinero.Crear(request.Monto, request.Moneda);

        var prestamo = Prestamo.Crear(
            capital,
            request.TasaMensual,
            request.NCuotas,
            request.FechaInicio,
            request.Descripcion);

        await _prestamoRepo.AgregarAsync(prestamo, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(prestamo.Id);
    }
}
