using FluentValidation;

namespace Feedlot.Application.Features.Compras.Commands.CrearCompra;

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
