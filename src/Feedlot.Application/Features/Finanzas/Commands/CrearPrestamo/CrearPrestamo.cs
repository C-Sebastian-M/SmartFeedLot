using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearPrestamo;

public sealed record CrearPrestamoCommand(
    decimal Monto,
    string Moneda,
    decimal TasaMensual,
    int NCuotas,
    DateOnly FechaInicio,
    string Descripcion
) : ICommand<Guid>;
public sealed class CrearPrestamoCommandValidator
    : AbstractValidator<CrearPrestamoCommand>
{
    public CrearPrestamoCommandValidator()
    {
        RuleFor(x => x.Monto)
            .GreaterThan(0).WithMessage("El monto del préstamo debe ser mayor a cero.");

        RuleFor(x => x.Moneda)
            .NotEmpty().Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.TasaMensual)
            .GreaterThanOrEqualTo(0).WithMessage("La tasa mensual no puede ser negativa.");

        RuleFor(x => x.NCuotas)
            .GreaterThan(0).WithMessage("El número de cuotas debe ser al menos 1.");

        RuleFor(x => x.FechaInicio)
            .NotEmpty().WithMessage("La fecha de inicio es requerida.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}

public sealed class CrearPrestamoCommandHandler
    : IRequestHandler<CrearPrestamoCommand, Result<Guid>>
{
    private readonly IPrestamoRepository _prestamoRepo;

    public CrearPrestamoCommandHandler(
        IPrestamoRepository prestamoRepo)
    {
        _prestamoRepo = prestamoRepo;
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

        return Result<Guid>.Success(prestamo.Id);
    }
}
