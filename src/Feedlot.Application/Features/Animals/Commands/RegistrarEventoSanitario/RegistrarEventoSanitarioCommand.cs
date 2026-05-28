using Feedlot.Application.Common;

namespace Feedlot.Application.Features.Animals.Commands.RegistrarEventoSanitario;

public sealed record RegistrarEventoSanitarioCommand(
    Guid AnimalId,
    DateOnly FechaEvento,
    string Diagnostico,
    string Descripcion,
    string Severidad,
    string? Tratamiento,
    string? TipoEvento,
    DateOnly? ProximaDosis,
    string? Responsable
) : ICommand<Guid>;
