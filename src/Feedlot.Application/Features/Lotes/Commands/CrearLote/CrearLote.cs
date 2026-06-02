using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Commands.CrearLote;

public sealed record CrearLoteCommand(
    string Nombre,
    int CapacidadMaxima
) : ICommand<Guid>;
public sealed class CrearLoteCommandValidator : AbstractValidator<CrearLoteCommand>
{
    public CrearLoteCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del lote es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.CapacidadMaxima)
            .GreaterThan(0).WithMessage("La capacidad máxima debe ser mayor a cero.")
            .LessThanOrEqualTo(10000).WithMessage("La capacidad máxima no puede superar 10.000 animales.");
    }
}

public sealed class CrearLoteCommandHandler
    : IRequestHandler<CrearLoteCommand, Result<Guid>>
{
    private readonly ILoteRepository _loteRepository;

    public CrearLoteCommandHandler(ILoteRepository loteRepository)
    {
        _loteRepository = loteRepository;
    }

    public async Task<Result<Guid>> Handle(CrearLoteCommand request, CancellationToken ct)
    {
        var codigo = await _loteRepository.ObtenerSiguienteCodigoAsync(ct);

        var lote = Lote.Crear(codigo, request.Nombre, request.CapacidadMaxima);

        await _loteRepository.AgregarAsync(lote, ct);

        return Result<Guid>.Success(lote.Id);
    }
}
