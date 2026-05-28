using FluentValidation;

namespace Feedlot.Application.Features.Ventas.Commands.CrearVenta;

public sealed class CrearVentaCommandValidator : AbstractValidator<CrearVentaCommand>
{
    public CrearVentaCommandValidator()
    {
        RuleFor(x => x.CompradorId)
            .NotEmpty().WithMessage("El comprador es requerido.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres.")
            .When(x => x.Descripcion is not null);

        RuleFor(x => x.Animales)
            .NotEmpty().WithMessage("Debe incluir al menos un animal en la venta.");

        RuleForEach(x => x.Animales).ChildRules(item =>
        {
            item.RuleFor(i => i.AnimalId)
                .NotEmpty().WithMessage("El ID del animal es requerido.");

            item.RuleFor(i => i.PrecioVenta)
                .GreaterThanOrEqualTo(0).WithMessage("El precio de venta no puede ser negativo.");

            item.RuleFor(i => i.PesoVentaKg)
                .GreaterThan(0).WithMessage("El peso de venta debe ser mayor a cero.");
        });
    }
}
