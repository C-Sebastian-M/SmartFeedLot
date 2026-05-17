using Feedlot.Application.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Services;
using MediatR;

namespace Feedlot.Application.Features.Lotes.Commands.MoverAnimalALote;

public sealed class MoverAnimalALoteCommandHandler
    : IRequestHandler<MoverAnimalALoteCommand, Result>
{
    private readonly AnimalLoteService _animalLoteService;

    public MoverAnimalALoteCommandHandler(AnimalLoteService animalLoteService)
    {
        _animalLoteService = animalLoteService;
    }

    public async Task<Result> Handle(MoverAnimalALoteCommand request, CancellationToken ct)
    {
        var motivo = Enum.Parse<MotivoMovimiento>(request.Motivo, ignoreCase: true);

        // El Domain Service valida la invariante de pertenencia única
        // y coordina el retiro del lote origen y el ingreso al destino.
        await _animalLoteService.MoverAnimalAsync(
            request.AnimalId,
            request.LoteDestinoId,
            request.FechaMovimiento,
            motivo,
            ct);

        return Result.Success();
    }
}
