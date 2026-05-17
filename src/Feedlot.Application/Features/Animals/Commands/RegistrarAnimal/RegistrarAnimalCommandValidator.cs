using FluentValidation;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarAnimal;

/// <summary>
/// Validador para RegistrarAnimalCommand.
/// FluentValidation — reglas declarativas, mensajes en español, fácil de extender.
/// El ValidationBehavior ejecuta esto antes de que el Handler sea invocado.
/// </summary>
public sealed class RegistrarAnimalCommandValidator
    : AbstractValidator<RegistrarAnimalCommand>
{
    private static readonly string[] SexosValidos = ["Macho", "Hembra"];
    private static readonly string[] MonedasValidas = ["COP", "USD", "EUR"];

    public RegistrarAnimalCommandValidator()
    {
        RuleFor(x => x.CodigoIdentificacion)
            .NotEmpty().WithMessage("El código de identificación es requerido.")
            .MinimumLength(3).WithMessage("El código debe tener al menos 3 caracteres.")
            .MaximumLength(20).WithMessage("El código no puede superar 20 caracteres.")
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage("El código solo puede contener letras, números y guiones.");

        RuleFor(x => x.NumeroArete)
            .NotEmpty().WithMessage("El número de arete es requerido.")
            .MaximumLength(50).WithMessage("El número de arete no puede superar 50 caracteres.");

        RuleFor(x => x.Sexo)
            .NotEmpty().WithMessage("El sexo es requerido.")
            .Must(s => SexosValidos.Contains(s))
            .WithMessage($"El sexo debe ser uno de: {string.Join(", ", SexosValidos)}.");

        RuleFor(x => x.Raza)
            .NotEmpty().WithMessage("La raza es requerida.")
            .MaximumLength(100).WithMessage("La raza no puede superar 100 caracteres.");

        RuleFor(x => x.FechaNacimiento)
            .NotEmpty().WithMessage("La fecha de nacimiento es requerida.")
            .Must(f => f < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de nacimiento debe ser anterior a hoy.");

        RuleFor(x => x.PesoIngresoKg)
            .GreaterThan(0).WithMessage("El peso de ingreso debe ser mayor a cero.")
            .LessThan(2000).WithMessage("El peso de ingreso parece inválido (máx 2000 kg).");

        RuleFor(x => x.PrecioCompra)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Must(m => MonedasValidas.Contains(m.ToUpperInvariant()))
            .WithMessage($"La moneda debe ser una de: {string.Join(", ", MonedasValidas)}.");

        RuleFor(x => x.FechaIngreso)
            .NotEmpty().WithMessage("La fecha de ingreso es requerida.")
            .GreaterThan(x => x.FechaNacimiento)
            .WithMessage("La fecha de ingreso debe ser posterior a la fecha de nacimiento.");
    }
}
