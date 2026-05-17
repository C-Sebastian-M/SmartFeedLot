using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarPesaje;

public sealed record RegistrarPesajeCommand(
    Guid AnimalId,
    DateOnly FechaPesaje,
    decimal PesoKg,
    string? Observaciones
) : IRequest<Result<Guid>>;
