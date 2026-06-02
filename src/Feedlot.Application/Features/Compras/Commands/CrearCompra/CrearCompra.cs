using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Compras.Commands.CrearCompra;

public sealed record CrearCompraCommand(
    Guid ProveedorId,
    DateOnly Fecha,
    string TipoCompra,
    decimal CostoTotal,
    string Moneda,
    string? Descripcion,
    int? CantidadCabezas,
    decimal? PrecioPorCabeza,
    decimal? PesoPromedioKg,
    Guid? LoteId,
    string? TipoInsumo,
    decimal? CantidadInsumo,
    string? UnidadMedida
) : ICommand<Guid>;
public sealed class CrearCompraCommandValidator : AbstractValidator<CrearCompraCommand>
{
    private static readonly string[] TiposValidos = ["Ganado", "Insumo"];

    public CrearCompraCommandValidator()
    {
        RuleFor(x => x.ProveedorId)
            .NotEmpty().WithMessage("El proveedor es requerido.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.");

        RuleFor(x => x.TipoCompra)
            .NotEmpty().WithMessage("El tipo de compra es requerido.")
            .Must(t => TiposValidos.Contains(t))
            .WithMessage($"El tipo de compra debe ser uno de: {string.Join(", ", TiposValidos)}.");

        RuleFor(x => x.CostoTotal)
            .GreaterThan(0).WithMessage("El costo total debe ser mayor a cero.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres.")
            .When(x => x.Descripcion is not null);

        When(x => x.TipoCompra == "Ganado", () =>
        {
            RuleFor(x => x.CantidadCabezas)
                .NotNull().WithMessage("La cantidad de cabezas es requerida para compras de ganado.")
                .GreaterThan(0).WithMessage("La cantidad de cabezas debe ser mayor a cero.");

            RuleFor(x => x.PrecioPorCabeza)
                .NotNull().WithMessage("El precio por cabeza es requerido para compras de ganado.")
                .GreaterThan(0).WithMessage("El precio por cabeza debe ser mayor a cero.");

            RuleFor(x => x.PesoPromedioKg)
                .NotNull().WithMessage("El peso promedio es requerido para compras de ganado.")
                .GreaterThan(0).WithMessage("El peso promedio debe ser mayor a cero.");

            RuleFor(x => x.LoteId)
                .NotNull().WithMessage("El lote de destino es requerido para compras de ganado.");
        });

        When(x => x.TipoCompra == "Insumo", () =>
        {
            RuleFor(x => x.TipoInsumo)
                .NotEmpty().WithMessage("El tipo de insumo es requerido para compras de insumo.")
                .MaximumLength(30).WithMessage("El tipo de insumo no puede superar 30 caracteres.");

            RuleFor(x => x.CantidadInsumo)
                .NotNull().WithMessage("La cantidad es requerida para compras de insumo.")
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

            RuleFor(x => x.UnidadMedida)
                .NotEmpty().WithMessage("La unidad de medida es requerida para compras de insumo.")
                .MaximumLength(20).WithMessage("La unidad de medida no puede superar 20 caracteres.");
        });
    }
}

public sealed class CrearCompraCommandHandler
    : IRequestHandler<CrearCompraCommand, Result<Guid>>
{
    private readonly ICompraRepository _compraRepository;

    public CrearCompraCommandHandler(ICompraRepository compraRepository)
    {
        _compraRepository = compraRepository;
    }

    public async Task<Result<Guid>> Handle(CrearCompraCommand request, CancellationToken ct)
    {
        Compra compra;
        if (request.TipoCompra == "Ganado")
        {
            if (!request.CantidadCabezas.HasValue || !request.PrecioPorCabeza.HasValue ||
                !request.PesoPromedioKg.HasValue || !request.LoteId.HasValue)
                return Result<Guid>.Validation("Para compras de ganado se requieren: cantidadCabezas, precioPorCabeza, pesoPromedioKg, loteId.");

            compra = Compra.CrearCompraGanado(
                request.ProveedorId, request.Fecha,
                request.CantidadCabezas.Value, request.PrecioPorCabeza.Value,
                request.PesoPromedioKg.Value, request.LoteId.Value,
                request.CostoTotal, request.Moneda, request.Descripcion);
        }
        else if (request.TipoCompra == "Insumo")
        {
            if (string.IsNullOrWhiteSpace(request.TipoInsumo) || !request.CantidadInsumo.HasValue ||
                string.IsNullOrWhiteSpace(request.UnidadMedida))
                return Result<Guid>.Validation("Para compras de insumo se requieren: tipoInsumo, cantidadInsumo, unidadMedida.");

            compra = Compra.CrearCompraInsumo(
                request.ProveedorId, request.Fecha,
                request.TipoInsumo, request.CantidadInsumo.Value,
                request.UnidadMedida, request.CostoTotal, request.Moneda,
                request.Descripcion);
        }
        else
        {
            return Result<Guid>.Validation("El tipo de compra debe ser 'Ganado' o 'Insumo'.");
        }

        await _compraRepository.AgregarAsync(compra, ct);
        return Result<Guid>.Success(compra.Id);
    }
}
