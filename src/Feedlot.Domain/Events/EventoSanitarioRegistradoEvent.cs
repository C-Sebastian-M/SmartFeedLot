using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;

namespace Feedlot.Domain.Events;

/// <summary>
/// Se emite cuando se registra un evento sanitario en un animal.
/// Si la severidad es Grave o Crítica, el contexto de Analítica genera una alerta.
/// </summary>
public sealed record EventoSanitarioRegistradoEvent(
    Guid AnimalId,
    Guid EventoId,
    string Diagnostico,
    int SeveridadNivel,
    DateOnly FechaEvento) : IDomainEvent;
