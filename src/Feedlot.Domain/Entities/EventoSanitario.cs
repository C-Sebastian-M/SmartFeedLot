using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Entity interna del aggregate Animal.
/// Registra un evento de salud: diagnóstico, severidad y tratamiento.
/// Solo se crea a través de Animal.RegistrarEventoSanitario().
/// </summary>
public sealed class EventoSanitario : Entity<Guid>
{
    private EventoSanitario() { } // EF Core

    private EventoSanitario(
        Guid id,
        Guid animalId,
        DateOnly fechaEvento,
        string diagnostico,
        string descripcion,
        SeveridadEvento severidad,
        string? tratamiento) : base(id)
    {
        AnimalId = animalId;
        FechaEvento = fechaEvento;
        Diagnostico = diagnostico;
        Descripcion = descripcion;
        Severidad = severidad;
        Tratamiento = tratamiento;
    }

    public Guid AnimalId { get; private set; }
    public DateOnly FechaEvento { get; private set; }
    public string Diagnostico { get; private set; } = null!;
    public string Descripcion { get; private set; } = null!;
    public SeveridadEvento Severidad { get; private set; }
    public string? Tratamiento { get; private set; }

    internal static EventoSanitario Crear(
        Guid animalId,
        DateOnly fechaEvento,
        string diagnostico,
        string descripcion,
        SeveridadEvento severidad,
        string? tratamiento)
    {
        return new EventoSanitario(
            Guid.NewGuid(), animalId, fechaEvento,
            diagnostico.Trim(), descripcion.Trim(),
            severidad, tratamiento?.Trim());
    }
}
