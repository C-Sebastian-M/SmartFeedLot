using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Commands.ActivarLote;

public sealed record ActivarLoteCommand(Guid LoteId) : ICommand;
public sealed class ActivarLoteCommandValidator : AbstractValidator<ActivarLoteCommand>
{
    public ActivarLoteCommandValidator()
    {
        RuleFor(x => x.LoteId)
            .NotEmpty().WithMessage("El ID del lote es requerido.");
    }
}

public sealed class ActivarLoteCommandHandler : IRequestHandler<ActivarLoteCommand, Result>
{
    private readonly ILoteRepository _loteRepository;

    public ActivarLoteCommandHandler(ILoteRepository loteRepository)
    {
        _loteRepository = loteRepository;
    }

    public async Task<Result> Handle(ActivarLoteCommand request, CancellationToken ct)
    {
        var lote = await _loteRepository.ObtenerPorIdAsync(request.LoteId, ct);

        if (lote is null)
            return Result.NotFound($"No se encontró el lote con ID '{request.LoteId}'.");

        // El dominio valida que solo se pueden activar lotes en EnPreparacion.
        // Si el estado no es válido lanza DomainException → HTTP 422.
        lote.Activar();

        _loteRepository.Actualizar(lote);

        return Result.Success();
    }
}
