using Feedlot.Application.Common;
using Feedlot.Domain.Entities;
using Feedlot.Domain.Interfaces;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Commands.CrearLote;

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
