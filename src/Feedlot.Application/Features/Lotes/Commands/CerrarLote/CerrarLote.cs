using Feedlot.Application.Common;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Commands.CerrarLote;

public sealed record CerrarLoteCommand(Guid LoteId) : ICommand;
public sealed class CerrarLoteCommandHandler : IRequestHandler<CerrarLoteCommand, Result>
{
    private readonly ILoteRepository _loteRepository;

    public CerrarLoteCommandHandler(ILoteRepository loteRepository)
    {
        _loteRepository = loteRepository;
    }

    public async Task<Result> Handle(CerrarLoteCommand request, CancellationToken ct)
    {
        var lote = await _loteRepository.ObtenerPorIdAsync(request.LoteId, ct);

        if (lote is null)
            return Result.NotFound($"No se encontró el lote con ID '{request.LoteId}'.");

        // El dominio valida que no haya animales activos antes de cerrar.
        lote.Cerrar();

        _loteRepository.Actualizar(lote);

        return Result.Success();
    }
}
