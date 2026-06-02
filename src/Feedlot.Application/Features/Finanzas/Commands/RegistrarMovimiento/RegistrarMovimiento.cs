using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.RegistrarMovimiento;

public sealed record RegistrarMovimientoFinancieroCommand(
    DateOnly Fecha,
    int PeriodoAnio,
    int PeriodoMes,
    Guid CategoriaGastoId,
    decimal Monto,
    string Moneda,
    string Origen,
    string Descripcion,
    Guid? SocioId,
    Guid RegistradoPorId
) : ICommand<Guid>;
public sealed class RegistrarMovimientoFinancieroCommandValidator
    : AbstractValidator<RegistrarMovimientoFinancieroCommand>
{
    public RegistrarMovimientoFinancieroCommandValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.PeriodoAnio)
            .InclusiveBetween(2000, 2100).WithMessage("El año del periodo no es válido.");

        RuleFor(x => x.PeriodoMes)
            .InclusiveBetween(1, 12).WithMessage("El mes del periodo debe estar entre 1 y 12.");

        RuleFor(x => x.CategoriaGastoId)
            .NotEmpty().WithMessage("La categoría de gasto es requerida.");

        RuleFor(x => x.Monto)
            .GreaterThan(0).WithMessage("El monto del movimiento debe ser mayor a cero.");

        RuleFor(x => x.Moneda)
            .NotEmpty().Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.Origen)
            .NotEmpty().WithMessage("El origen financiero es requerido.")
            .Must(o => Enum.TryParse<OrigenFinanciero>(o, ignoreCase: true, out _))
            .WithMessage("Origen financiero inválido. Valores válidos: Bovino, Porcino, Agricola, General.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");

        RuleFor(x => x.RegistradoPorId)
            .NotEmpty().WithMessage("El usuario que registra es requerido.");
    }
}

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
