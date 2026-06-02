using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.Services;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarAnimal;

public sealed record RegistrarAnimalCommand(
    string? Nombre,
    string Sexo,
    string? Raza,
    DateOnly? FechaNacimiento,
    decimal PesoIngresoKg,
    decimal PrecioCompraPorKg,
    string Moneda,
    DateOnly FechaIngreso,
    Guid? LoteInicialId
) : ICommand<Guid>;
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
        RuleFor(x => x.Nombre)
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.")
            .When(x => x.Nombre is not null);

        RuleFor(x => x.Sexo)
            .NotEmpty().WithMessage("El sexo es requerido.")
            .Must(s => SexosValidos.Contains(s))
            .WithMessage($"El sexo debe ser uno de: {string.Join(", ", SexosValidos)}.");

        RuleFor(x => x.Raza)
            .MaximumLength(100).WithMessage("La raza no puede superar 100 caracteres.")
            .When(x => x.Raza is not null);

        RuleFor(x => x.FechaNacimiento)
            .Must(f => !f.HasValue || f.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de nacimiento debe ser anterior a hoy.")
            .When(x => x.FechaNacimiento.HasValue);

        RuleFor(x => x.PesoIngresoKg)
            .GreaterThan(0).WithMessage("El peso de ingreso debe ser mayor a cero.")
            .LessThan(2000).WithMessage("El peso de ingreso parece inválido (máx 2000 kg).");

        RuleFor(x => x.PrecioCompraPorKg)
            .GreaterThanOrEqualTo(0).WithMessage("El precio por kilo no puede ser negativo.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Must(m => MonedasValidas.Contains(m.ToUpperInvariant()))
            .WithMessage($"La moneda debe ser una de: {string.Join(", ", MonedasValidas)}.");

        RuleFor(x => x.FechaIngreso)
            .NotEmpty().WithMessage("La fecha de ingreso es requerida.");
    }
}
/// <summary>
/// Handler para RegistrarAnimalCommand.
/// 
/// Responsabilidades del Handler:
/// 1. Traducir el Command a objetos de dominio (Value Objects).
/// 2. Invocar el factory method del Aggregate Root.
/// 3. Persistir usando el repositorio.
/// 4. Si se especificó un lote inicial, coordinar el ingreso vía Domain Service.
/// 5. Retornar el ID del animal creado.
/// 
/// El Handler NO valida reglas de negocio — eso es responsabilidad del Dominio.
/// El Handler NO persiste directamente — el UnitOfWorkBehavior lo hace al final del pipeline.
/// </summary>
public sealed class RegistrarAnimalCommandHandler
    : IRequestHandler<RegistrarAnimalCommand, Result<Guid>>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly AnimalLoteService _animalLoteService;

    public RegistrarAnimalCommandHandler(
        IAnimalRepository animalRepository,
        AnimalLoteService animalLoteService)
    {
        _animalRepository = animalRepository;
        _animalLoteService = animalLoteService;
    }

    public async Task<Result<Guid>> Handle(
        RegistrarAnimalCommand request,
        CancellationToken ct)
    {
        var codigoStr = await _animalRepository.ObtenerSiguienteCodigoAsync(ct);
        var codigo = CodigoIdentificacion.Crear(codigoStr);

        var arete = await _animalRepository.ObtenerSiguienteAreteAsync(ct);

        var pesoIngreso = Peso.Crear(request.PesoIngresoKg);
        var precioCompra = Dinero.Crear(
            request.PrecioCompraPorKg * request.PesoIngresoKg, request.Moneda);
        var sexo = Enum.Parse<Sexo>(request.Sexo, ignoreCase: true);

        var animal = Animal.Registrar(
            codigo,
            request.Nombre,
            arete,
            sexo,
            request.Raza,
            request.FechaNacimiento,
            pesoIngreso,
            precioCompra,
            request.FechaIngreso);

        await _animalRepository.AgregarAsync(animal, ct);

        if (request.LoteInicialId.HasValue)
        {
            await _animalLoteService.IngresoInicialAsync(
                animal.Id,
                request.LoteInicialId.Value,
                request.FechaIngreso,
                ct);
        }

        return Result<Guid>.Success(animal.Id);
    }
}
