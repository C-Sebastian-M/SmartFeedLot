using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Finanzas.Commands.CrearCategoriaGasto;

public sealed record CrearCategoriaGastoCommand(
    string Nombre,
    string Tipo
) : ICommand<Guid>;
public sealed class CrearCategoriaGastoCommandValidator
    : AbstractValidator<CrearCategoriaGastoCommand>
{
    public CrearCategoriaGastoCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la categoría es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo de categoría es requerido.")
            .Must(t => Enum.TryParse<TipoCategoriaGasto>(t, ignoreCase: true, out _))
            .WithMessage("Tipo de categoría inválido. Valores válidos: Directo, Indirecto, Operativo, Inversion.");
    }
}

public sealed class CrearCategoriaGastoCommandHandler
    : IRequestHandler<CrearCategoriaGastoCommand, Result<Guid>>
{
    private readonly ICategoriaGastoRepository _categoriaRepo;

    public CrearCategoriaGastoCommandHandler(
        ICategoriaGastoRepository categoriaRepo)
    {
        _categoriaRepo = categoriaRepo;
    }

    public async Task<Result<Guid>> Handle(
        CrearCategoriaGastoCommand request,
        CancellationToken ct)
    {
        var existe = await _categoriaRepo.ObtenerPorNombreAsync(request.Nombre, ct);
        if (existe is not null)
            return Result<Guid>.Failure(
                $"Ya existe una categoría de gasto con el nombre '{request.Nombre}'.");

        if (!Enum.TryParse<TipoCategoriaGasto>(request.Tipo, ignoreCase: true, out var tipo))
            return Result<Guid>.Failure("Tipo de categoría de gasto inválido.");

        var categoria = CategoriaGasto.Crear(request.Nombre, tipo);

        await _categoriaRepo.AgregarAsync(categoria, ct);

        return Result<Guid>.Success(categoria.Id);
    }
}
