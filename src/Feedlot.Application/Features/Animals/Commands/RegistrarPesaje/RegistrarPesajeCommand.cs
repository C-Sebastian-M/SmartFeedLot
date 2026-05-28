using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarPesaje;

public sealed record RegistrarPesajeCommand(
    Guid AnimalId,
    DateOnly FechaPesaje,
    decimal PesoKg,
    string? Observaciones
) : ICommand<Guid>;
