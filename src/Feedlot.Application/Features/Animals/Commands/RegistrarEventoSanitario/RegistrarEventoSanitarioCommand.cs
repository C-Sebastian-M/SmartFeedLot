using Feedlot.Application.Common;
using MediatR;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarEventoSanitario;

public sealed record RegistrarEventoSanitarioCommand(
    Guid AnimalId,
    DateOnly FechaEvento,
    string Diagnostico,
    string Descripcion,
    string Severidad,
    string? Tratamiento
) : IRequest<Result<Guid>>;
