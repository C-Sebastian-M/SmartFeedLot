using FluentValidation;

namespace Feedlot.Application.Features.Animals.Commands.ModificarAnimal;

public sealed class ModificarAnimalCommandValidator
    : AbstractValidator<ModificarAnimalCommand>
{
    private static readonly string[] SexosValidos = ["Macho", "Hembra"];
    private static readonly string[] MonedasValidas = ["COP", "USD", "EUR"];

    public ModificarAnimalCommandValidator()
    {
        RuleFor(x => x.AnimalId)
            .NotEmpty().WithMessage("El ID del animal es requerido.");

        RuleFor(x => x.Nombre)
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.")
            .When(x => x.Nombre is not null);

        RuleFor(x => x.NumeroArete)
            .NotEmpty().WithMessage("El número de arete es requerido.")
            .MaximumLength(50).WithMessage("El número de arete no puede superar 50 caracteres.");

        RuleFor(x => x.Sexo)
            .NotEmpty().WithMessage("El sexo es requerido.")
            .Must(s => SexosValidos.Contains(s))
            .WithMessage($"El sexo debe ser uno de: {string.Join(", ", SexosValidos)}.");

        RuleFor(x => x.Raza)
            .MaximumLength(100).WithMessage("La raza no puede superar 100 caracteres.");

        RuleFor(x => x.FechaNacimiento)
            .Must(f => !f.HasValue || f.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de nacimiento debe ser anterior a hoy.")
            .When(x => x.FechaNacimiento.HasValue);

        RuleFor(x => x.FechaIngreso)
            .NotEmpty().WithMessage("La fecha de ingreso es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de ingreso no puede ser futura.");

        RuleFor(x => x.PesoIngresoKg)
            .GreaterThan(0).WithMessage("El peso de ingreso debe ser mayor a cero.")
            .LessThan(2000).WithMessage("El peso de ingreso parece inválido (máx 2000 kg).");

        RuleFor(x => x.PrecioCompra)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Must(m => MonedasValidas.Contains(m.ToUpperInvariant()))
            .WithMessage($"La moneda debe ser una de: {string.Join(", ", MonedasValidas)}.");

        RuleFor(x => x.NuevoLoteId)
            .NotEmpty().WithMessage("El ID del lote destino no puede ser vacío.")
            .When(x => x.NuevoLoteId.HasValue);
    }
}
