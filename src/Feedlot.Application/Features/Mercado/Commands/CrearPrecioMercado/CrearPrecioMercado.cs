using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Mercado.Commands.CrearPrecioMercado;

public sealed record CrearPrecioMercadoCommand(
    DateOnly Fecha,
    string Especie,
    string Tipo,
    decimal PrecioPorKg,
    string Fuente
) : ICommand<Guid>;
public sealed class CrearPrecioMercadoCommandValidator : AbstractValidator<CrearPrecioMercadoCommand>
{
    public CrearPrecioMercadoCommandValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.");

        RuleFor(x => x.Especie)
            .NotEmpty().WithMessage("La especie es requerida.")
            .MaximumLength(100).WithMessage("La especie no puede superar 100 caracteres.");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo es requerido.")
            .MaximumLength(100).WithMessage("El tipo no puede superar 100 caracteres.");

        RuleFor(x => x.PrecioPorKg)
            .GreaterThan(0).WithMessage("El precio por kg debe ser mayor a cero.");

        RuleFor(x => x.Fuente)
            .NotEmpty().WithMessage("La fuente es requerida.")
            .MaximumLength(200).WithMessage("La fuente no puede superar 200 caracteres.");
    }
}

public sealed class CrearPrecioMercadoCommandHandler
    : IRequestHandler<CrearPrecioMercadoCommand, Result<Guid>>
{
    private readonly IPrecioMercadoRepository _precioMercadoRepository;

    public CrearPrecioMercadoCommandHandler(IPrecioMercadoRepository precioMercadoRepository)
    {
        _precioMercadoRepository = precioMercadoRepository;
    }

    public async Task<Result<Guid>> Handle(CrearPrecioMercadoCommand request, CancellationToken ct)
    {
        var precio = PrecioMercado.Crear(request.Fecha, request.Especie, request.Tipo, request.PrecioPorKg, request.Fuente);
        await _precioMercadoRepository.AgregarAsync(precio, ct);
        return Result<Guid>.Success(precio.Id);
    }
}
