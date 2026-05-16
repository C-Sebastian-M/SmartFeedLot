using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Events;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Aggregate Root: Lote.
/// Representa un grupo de animales en proceso de engorde colectivo.
/// 
/// Invariantes que protege:
/// - No puede exceder su capacidad máxima.
/// - Un animal no puede estar en dos lotes activos simultáneamente
///   (esta regla se verifica en el Domain Service LoteService, que coordina
///   ambos aggregates a través de sus repositorios).
/// - Solo lotes activos aceptan nuevos animales.
/// </summary>
public sealed class Lote : AggregateRoot<Guid>
{
    private readonly List<AnimalLote> _animalesLote = [];

    private Lote() { } // EF Core

    private Lote(
        Guid id,
        string codigo,
        string nombre,
        Capacidad capacidad) : base(id)
    {
        Codigo = codigo;
        Nombre = nombre;
        Capacidad = capacidad;
        Estado = EstadoLote.EnPreparacion;
    }

    public string Codigo { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public Capacidad Capacidad { get; private set; } = null!;
    public EstadoLote Estado { get; private set; }

    public IReadOnlyCollection<AnimalLote> AnimalesLote => _animalesLote.AsReadOnly();

    // --- Factory ---

    public static Lote Crear(string codigo, string nombre, int capacidadMaxima)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("El código del lote no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del lote no puede estar vacío.");

        var capacidad = Capacidad.Crear(capacidadMaxima);

        return new Lote(Guid.NewGuid(), codigo.Trim().ToUpperInvariant(), nombre.Trim(), capacidad);
    }

    // --- Comportamiento ---

    /// <summary>
    /// Activa el lote para que pueda recibir animales.
    /// Solo puede activarse desde EnPreparacion.
    /// </summary>
    public void Activar()
    {
        if (Estado != EstadoLote.EnPreparacion)
            throw new DomainException(
                $"Solo se puede activar un lote que esté en preparación. Estado actual: {Estado}.");

        Estado = EstadoLote.Activo;
    }

    /// <summary>
    /// Agrega un animal al lote. Verifica capacidad y estado del lote.
    /// La invariante de "animal en un solo lote activo" es responsabilidad
    /// del Domain Service, que tiene acceso a ambos repositorios.
    /// </summary>
    public AnimalLote AgregarAnimal(Guid animalId, DateOnly fechaIngreso, MotivoMovimiento motivo)
    {
        if (Estado != EstadoLote.Activo)
            throw new DomainException(
                $"Solo se pueden agregar animales a lotes activos. Estado actual: {Estado}.");

        if (!Capacidad.TieneEspacio)
            throw new LoteCapacidadExcedidaException(Id, Capacidad.Maxima);

        var yaEstaEnEsteLote = _animalesLote
            .Any(al => al.AnimalId == animalId && al.EsActivo);

        if (yaEstaEnEsteLote)
            throw new DomainException(
                $"El animal '{animalId}' ya se encuentra activo en este lote.");

        var animalLote = AnimalLote.Crear(Id, animalId, fechaIngreso, motivo);
        _animalesLote.Add(animalLote);
        Capacidad = Capacidad.ConAnimalAgregado();

        RaiseDomainEvent(new AnimalMovidoALoteEvent(animalId, null, Id, fechaIngreso, motivo));

        return animalLote;
    }

    /// <summary>
    /// Retira un animal del lote registrando la fecha de egreso.
    /// </summary>
    public void RetirarAnimal(Guid animalId, DateOnly fechaEgreso, MotivoMovimiento motivo)
    {
        var animalLote = _animalesLote
            .FirstOrDefault(al => al.AnimalId == animalId && al.EsActivo)
            ?? throw new DomainException(
                $"El animal '{animalId}' no se encuentra activo en el lote '{Id}'.");

        animalLote.Cerrar(fechaEgreso, motivo);
        Capacidad = Capacidad.ConAnimalRetirado();

        RaiseDomainEvent(new AnimalMovidoALoteEvent(animalId, Id, null, fechaEgreso, motivo));
    }

    /// <summary>Cierra el lote. Solo si no tiene animales activos.</summary>
    public void Cerrar()
    {
        var tieneAnimalesActivos = _animalesLote.Any(al => al.EsActivo);
        if (tieneAnimalesActivos)
            throw new DomainException(
                "No se puede cerrar un lote con animales activos. Retire todos los animales primero.");

        if (Estado == EstadoLote.Cerrado)
            throw new DomainException("El lote ya está cerrado.");

        Estado = EstadoLote.Cerrado;
    }

    public bool EstaActivo => Estado == EstadoLote.Activo;
    public int CantidadAnimalesActivos => _animalesLote.Count(al => al.EsActivo);
}
