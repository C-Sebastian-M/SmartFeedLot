using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using Feedlot.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Nutricion.Commands.RegistrarConsumo;

public sealed record RegistrarConsumoCommand(
    Guid LoteId,
    Guid RacionId,
    DateOnly Fecha,
    decimal CantidadKg,
    decimal CostoTotal,
    string Moneda,
    Guid RegistradoPorId
) : ICommand<Guid>;
public sealed class RegistrarConsumoCommandValidator
    : AbstractValidator<RegistrarConsumoCommand>
{
    public RegistrarConsumoCommandValidator()
    {
        RuleFor(x => x.LoteId)
            .NotEmpty().WithMessage("El ID del lote es requerido.");

        RuleFor(x => x.RacionId)
            .NotEmpty().WithMessage("El ID de la ración es requerido.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha del consumo es requerida.")
            .Must(f => f <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha del consumo no puede ser futura.");

        RuleFor(x => x.CantidadKg)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La cantidad de kilogramos no puede ser negativa.");

        RuleFor(x => x.CostoTotal)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El costo total no puede ser negativo.");

        RuleFor(x => x.Moneda)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .Length(3).WithMessage("La moneda debe ser un código ISO de 3 caracteres.");

        RuleFor(x => x.RegistradoPorId)
            .NotEmpty().WithMessage("El ID del usuario que registra es requerido.");
    }
}

public sealed class RegistrarConsumoCommandHandler
    : IRequestHandler<RegistrarConsumoCommand, Result<Guid>>
{
    private readonly IConsumoAlimenticioRepository _consumoRepository;
    private readonly ILoteRepository _loteRepository;
    private readonly IRacionRepository _racionRepository;

    public RegistrarConsumoCommandHandler(
        IConsumoAlimenticioRepository consumoRepository,
        ILoteRepository loteRepository,
        IRacionRepository racionRepository)
    {
        _consumoRepository = consumoRepository;
        _loteRepository = loteRepository;
        _racionRepository = racionRepository;
    }

    public async Task<Result<Guid>> Handle(RegistrarConsumoCommand request, CancellationToken ct)
    {
        // Verificar que el lote existe y está activo.
        var lote = await _loteRepository.ObtenerPorIdAsync(request.LoteId, ct);
        if (lote is null)
            return Result<Guid>.NotFound($"No se encontró el lote con ID '{request.LoteId}'.");

        if (!lote.EstaActivo)
            return Result<Guid>.Failure(
                $"El lote '{lote.Codigo}' no está activo. No se puede registrar consumo.");

        // Verificar que la ración existe y está activa.
        var racion = await _racionRepository.ObtenerPorIdAsync(request.RacionId, ct);
        if (racion is null)
            return Result<Guid>.NotFound($"No se encontró la ración con ID '{request.RacionId}'.");

        if (!racion.Activa)
            return Result<Guid>.Failure(
                $"La ración '{racion.Nombre}' está inactiva. Seleccione una ración activa.");

        var cantidad = CantidadKilogramos.Crear(request.CantidadKg);
        var costo = Dinero.Crear(request.CostoTotal, request.Moneda);

        var consumo = ConsumoAlimenticio.Registrar(
            request.LoteId,
            request.RacionId,
            request.Fecha,
            cantidad,
            costo,
            request.RegistradoPorId);

        await _consumoRepository.AgregarAsync(consumo, ct);

        return Result<Guid>.Success(consumo.Id);
    }
}
