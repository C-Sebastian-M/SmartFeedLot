using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Entity interna del aggregate Animal.
/// Representa un pesaje puntual en el tiempo.
/// Solo se crea a través del método Animal.RegistrarPesaje() para garantizar
/// que las invariantes del aggregate sean respetadas.
/// </summary>
public sealed class Pesaje : Entity<Guid>
{
    private Pesaje() { } // EF Core

    private Pesaje(
        Guid id,
        Guid animalId,
        DateOnly fechaPesaje,
        Peso peso,
        string? observaciones) : base(id)
    {
        AnimalId = animalId;
        FechaPesaje = fechaPesaje;
        Peso = peso;
        Observaciones = observaciones;
    }

    public Guid AnimalId { get; private set; }
    public DateOnly FechaPesaje { get; private set; }
    public Peso Peso { get; private set; } = null!;
    public string? Observaciones { get; private set; }

    internal static Pesaje Crear(
        Guid animalId,
        DateOnly fechaPesaje,
        Peso peso,
        string? observaciones)
    {
        return new Pesaje(Guid.NewGuid(), animalId, fechaPesaje, peso, observaciones);
    }
}
