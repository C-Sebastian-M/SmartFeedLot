using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Nutricion.Commands.CrearRacion;

public sealed record CrearRacionCommand(
    string Nombre,
    decimal CostoKg,
    string Moneda,
    decimal ProteinaPct,
    decimal EnergiaMcal
) : ICommand<Guid>;
public sealed class CrearRacionCommandValidator : AbstractValidator<CrearRacionCommand>
{
    public CrearRacionCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la ración es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede superar 150 caracteres.");

        RuleFor(x => x.CostoKg)
            .GreaterThan(0).WithMessage("El costo por kilogramo debe ser mayor a cero.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.ProteinaPct)
            .InclusiveBetween(0, 100)
            .WithMessage("El porcentaje de proteína debe estar entre 0 y 100.");

        RuleFor(x => x.EnergiaMcal)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La energía no puede ser negativa.");
    }
}

public sealed class CrearRacionCommandHandler
    : IRequestHandler<CrearRacionCommand, Result<Guid>>
{
    private readonly IRacionRepository _racionRepository;

    public CrearRacionCommandHandler(IRacionRepository racionRepository)
    {
        _racionRepository = racionRepository;
    }

    public async Task<Result<Guid>> Handle(CrearRacionCommand request, CancellationToken ct)
    {
        var nombreExiste = await _racionRepository
            .ExisteNombreAsync(request.Nombre, ct);

        if (nombreExiste)
            return Result<Guid>.Conflict(
                $"Ya existe una ración con el nombre '{request.Nombre}'.");

        var costoKg = Dinero.Crear(request.CostoKg, request.Moneda);

        var racion = Racion.Crear(
            request.Nombre,
            costoKg,
            request.ProteinaPct,
            request.EnergiaMcal);

        await _racionRepository.AgregarAsync(racion, ct);

        return Result<Guid>.Success(racion.Id);
    }
}
