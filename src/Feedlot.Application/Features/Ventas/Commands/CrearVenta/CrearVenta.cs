using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Ventas.Commands.CrearVenta;

public sealed record VentaAnimalInput(
    Guid AnimalId,
    decimal PrecioVenta,
    decimal PesoVentaKg);

public sealed record CrearVentaCommand(
    Guid CompradorId,
    DateOnly Fecha,
    string Moneda,
    string? Descripcion,
    List<VentaAnimalInput> Animales
) : ICommand<Guid>;
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

public sealed class CrearVentaCommandHandler
    : IRequestHandler<CrearVentaCommand, Result<Guid>>
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly ILoteRepository _loteRepository;

    public CrearVentaCommandHandler(
        IVentaRepository ventaRepository,
        IAnimalRepository animalRepository,
        ILoteRepository loteRepository)
    {
        _ventaRepository = ventaRepository;
        _animalRepository = animalRepository;
        _loteRepository = loteRepository;
    }

    public async Task<Result<Guid>> Handle(CrearVentaCommand request, CancellationToken ct)
    {
        var venta = Venta.Crear(request.CompradorId, request.Fecha, request.Moneda, request.Descripcion);

        foreach (var animalInput in request.Animales)
        {
            var animal = await _animalRepository.ObtenerPorIdAsync(animalInput.AnimalId, ct);
            if (animal is null)
                return Result<Guid>.NotFound($"Animal {animalInput.AnimalId} no encontrado.");

            if (!animal.EstaActivo)
                return Result<Guid>.Validation($"El animal {animal.CodigoIdentificacion.Valor} no está activo (estado: {animal.EstadoProductivo}).");

            venta.AgregarItem(animalInput.AnimalId, animalInput.PrecioVenta, animalInput.PesoVentaKg);

            animal.MarcarComoVendido();

            var loteActivo = await _loteRepository.ObtenerLoteActivoDelAnimalAsync(animalInput.AnimalId, ct);
            if (loteActivo is not null)
            {
                loteActivo.RetirarAnimal(animalInput.AnimalId, request.Fecha, MotivoMovimiento.Venta);
                _loteRepository.Actualizar(loteActivo);
            }
        }

        await _ventaRepository.AgregarAsync(venta, ct);
        return Result<Guid>.Success(venta.Id);
    }
}
