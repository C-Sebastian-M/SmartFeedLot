using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Lotes.Commands.MoverAnimalALote;

public sealed record MoverAnimalALoteCommand(
    Guid AnimalId,
    Guid LoteDestinoId,
    DateOnly FechaMovimiento,
    string Motivo
) : ICommand;
