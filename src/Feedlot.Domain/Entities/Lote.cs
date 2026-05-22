using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Events;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Aggregate Root: Lote.
/// Capacidad se computa en tiempo real desde AnimalesLote.
/// CapacidadMaxima se expone como propiedad con setter privado
/// para que EF Core pueda leerla y escribirla sin reflexión.
/// </summary>
public sealed class Lote : AggregateRoot<Guid>
{
    private readonly List<AnimalLote> _animalesLote = [];

    private Lote() { } // EF Core

    private Lote(
        Guid id,
        string codigo,
        string nombre,
        int capacidadMaxima) : base(id)
    {
        Codigo = codigo;
        Nombre = nombre;
        CapacidadMaxima = capacidadMaxima;
        Estado = EstadoLote.EnPreparacion;
    }

    public string Codigo { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public EstadoLote Estado { get; private set; }

    /// <summary>
    /// Capacidad máxima persistida. EF Core mapea esta propiedad directamente.
    /// </summary>
    public int CapacidadMaxima { get; private set; }

    /// <summary>
    /// Capacidad calculada en tiempo real desde AnimalesLote.
    /// Actual = count de animales activos. Nunca desincronizada con BD.
    /// No se persiste — se ignora en la configuración EF Core.
    /// </summary>
    public Capacidad Capacidad
    {
        get
        {
            var actual = _animalesLote.Count(al => al.EsActivo);
            return Capacidad.Crear(CapacidadMaxima, actual);
        }
    }

    public IReadOnlyCollection<AnimalLote> AnimalesLote => _animalesLote.AsReadOnly();

    // --- Factory ---
    public static Lote Crear(string codigo, string nombre, int capacidadMaxima)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("El código del lote no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del lote no puede estar vacío.");

        if (capacidadMaxima <= 0)
            throw new DomainException("La capacidad máxima debe ser mayor a cero.");

        return new Lote(
            Guid.NewGuid(),
            codigo.Trim().ToUpperInvariant(),
            nombre.Trim(),
            capacidadMaxima);
    }

    // --- Comportamiento ---
    public void Activar()
    {
        if (Estado != EstadoLote.EnPreparacion)
            throw new DomainException(
                $"Solo se puede activar un lote en preparación. Estado actual: {Estado}.");
        Estado = EstadoLote.Activo;
    }

    public AnimalLote AgregarAnimal(Guid animalId, DateOnly fechaIngreso, MotivoMovimiento motivo)
    {
        if (Estado != EstadoLote.Activo)
            throw new DomainException(
                $"Solo lotes activos aceptan animales. Estado actual: {Estado}.");

        if (!Capacidad.TieneEspacio)
            throw new LoteCapacidadExcedidaException(Id, CapacidadMaxima);

        var yaEstaEnEsteLote = _animalesLote
            .Any(al => al.AnimalId == animalId && al.EsActivo);

        if (yaEstaEnEsteLote)
            throw new DomainException(
                $"El animal '{animalId}' ya se encuentra activo en este lote.");

        var animalLote = AnimalLote.Crear(Id, animalId, fechaIngreso, motivo);
        _animalesLote.Add(animalLote);

        RaiseDomainEvent(new AnimalMovidoALoteEvent(animalId, null, Id, fechaIngreso, motivo));

        return animalLote;
    }

    public void RetirarAnimal(Guid animalId, DateOnly fechaEgreso, MotivoMovimiento motivo)
    {
        var animalLote = _animalesLote
            .FirstOrDefault(al => al.AnimalId == animalId && al.EsActivo)
            ?? throw new DomainException(
                $"El animal '{animalId}' no se encuentra activo en el lote '{Id}'.");

        animalLote.Cerrar(fechaEgreso, motivo);

        RaiseDomainEvent(new AnimalMovidoALoteEvent(animalId, Id, null, fechaEgreso, motivo));
    }

    public void Cerrar()
    {
        if (_animalesLote.Any(al => al.EsActivo))
            throw new DomainException(
                "No se puede cerrar un lote con animales activos.");

        if (Estado == EstadoLote.Cerrado)
            throw new DomainException("El lote ya está cerrado.");

        Estado = EstadoLote.Cerrado;
    }

    public bool EstaActivo => Estado == EstadoLote.Activo;
    public int CantidadAnimalesActivos => _animalesLote.Count(al => al.EsActivo);
}
