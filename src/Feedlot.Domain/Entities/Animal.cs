using Feedlot.Domain.Common;
using Feedlot.Domain.Enums;
using Feedlot.Domain.Events;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Aggregate Root: Animal.
/// Representa un bovino individual dentro del sistema de feedlot.
/// Es el límite de consistencia para Pesajes y EventosSanitarios.
/// 
/// Invariantes que protege:
/// - Solo animales activos (EnEngorde) pueden registrar eventos.
/// - Los pesajes deben mantener orden cronológico estricto.
/// - El estado productivo controla el ciclo de vida del animal.
/// </summary>
public sealed class Animal : AggregateRoot<Guid>
{
    private readonly List<Pesaje> _pesajes = [];
    private readonly List<EventoSanitario> _eventosSanitarios = [];

    private Animal() { } // EF Core

    private Animal(
        Guid id,
        CodigoIdentificacion codigoIdentificacion,
        string? nombre,
        string numeroArete,
        Sexo sexo,
        string? raza,
        DateOnly? fechaNacimiento,
        Peso pesoIngreso,
        Dinero precioCompra,
        DateOnly fechaIngreso) : base(id)
    {
        CodigoIdentificacion = codigoIdentificacion;
        Nombre = nombre;
        NumeroArete = numeroArete;
        Sexo = sexo;
        Raza = raza?.Trim();
        FechaNacimiento = fechaNacimiento;
        PesoIngreso = pesoIngreso;
        PrecioCompra = precioCompra;
        FechaIngreso = fechaIngreso;
        EstadoProductivo = EstadoProductivo.EnEngorde;
        EstadoSanitario = EstadoSanitario.Sano;
    }

    // --- Propiedades ---
    public CodigoIdentificacion CodigoIdentificacion { get; private set; } = null!;
    public string? Nombre { get; private set; }
    public string NumeroArete { get; private set; } = null!;
    public Sexo Sexo { get; private set; }
    public string? Raza { get; private set; }
    public DateOnly? FechaNacimiento { get; private set; }
    public Peso PesoIngreso { get; private set; } = null!;
    public Dinero PrecioCompra { get; private set; } = null!;
    public DateOnly FechaIngreso { get; private set; }
    public EstadoProductivo EstadoProductivo { get; private set; }
    public EstadoSanitario EstadoSanitario { get; private set; }

    public IReadOnlyCollection<Pesaje> Pesajes => _pesajes.AsReadOnly();
    public IReadOnlyCollection<EventoSanitario> EventosSanitarios => _eventosSanitarios.AsReadOnly();

    // --- Factory Method ---

    /// <summary>
    /// Crea un nuevo animal y emite el domain event AnimalRegistrado.
    /// Toda la lógica de creación pasa por aquí — nunca por el constructor directamente.
    /// </summary>
    public static Animal Registrar(
        CodigoIdentificacion codigoIdentificacion,
        string? nombre,
        string numeroArete,
        Sexo sexo,
        string? raza,
        DateOnly? fechaNacimiento,
        Peso pesoIngreso,
        Dinero precioCompra,
        DateOnly fechaIngreso)
    {
        if (string.IsNullOrWhiteSpace(numeroArete))
            throw new DomainException("El número de arete no puede estar vacío.");

        if (fechaNacimiento.HasValue && fechaNacimiento.Value >= fechaIngreso)
            throw new DomainException(
                "La fecha de ingreso debe ser posterior a la fecha de nacimiento.");

        var animal = new Animal(
            Guid.NewGuid(),
            codigoIdentificacion,
            nombre?.Trim(),
            numeroArete.Trim().ToUpperInvariant(),
            sexo,
            raza?.Trim(),
            fechaNacimiento,
            pesoIngreso,
            precioCompra,
            fechaIngreso);

        animal.RaiseDomainEvent(new AnimalRegistradoEvent(
            animal.Id,
            animal.CodigoIdentificacion.Valor,
            animal.PesoIngreso.Kilogramos,
            animal.FechaIngreso));

        return animal;
    }

    /// <summary>
    /// Modifica los datos editables del animal.
    /// No cambia identidad, estado productivo, fechas de ingreso ni colecciones.
    /// </summary>
    public void Modificar(
        string? nombre,
        string numeroArete,
        string? raza,
        DateOnly? fechaNacimiento,
        DateOnly fechaIngreso,
        Peso pesoIngreso,
        Dinero precioCompra)
    {
        if (string.IsNullOrWhiteSpace(numeroArete))
            throw new DomainException("El número de arete no puede estar vacío.");

        if (fechaNacimiento.HasValue && fechaIngreso <= fechaNacimiento.Value)
            throw new DomainException("La fecha de nacimiento debe ser anterior a la fecha de ingreso.");

        Nombre = nombre?.Trim();
        NumeroArete = numeroArete.Trim().ToUpperInvariant();
        Raza = raza?.Trim();
        FechaNacimiento = fechaNacimiento;
        FechaIngreso = fechaIngreso;
        PesoIngreso = pesoIngreso;
        PrecioCompra = precioCompra;
    }

    // --- Comportamiento de dominio ---

    /// <summary>
    /// Registra un nuevo pesaje sobre el animal.
    /// Aplica invariantes: animal activo + orden cronológico.
    /// </summary>
    /// <summary>
    /// Elimina un pesaje del historial.
    /// Solo permite eliminar si el animal está activo.
    /// </summary>
    public void EliminarPesaje(Guid pesajeId)
    {
        ValidarAnimalActivo();

        var pesaje = _pesajes.FirstOrDefault(p => p.Id == pesajeId);
        if (pesaje is null)
            throw new DomainException("El pesaje no existe en este animal.");

        _pesajes.Remove(pesaje);
    }

    public Pesaje RegistrarPesaje(DateOnly fechaPesaje, Peso peso, string? observaciones = null)
    {
        ValidarAnimalActivo();

        var ultimoPesaje = _pesajes
            .OrderByDescending(p => p.FechaPesaje)
            .FirstOrDefault();

        if (ultimoPesaje is not null && fechaPesaje <= ultimoPesaje.FechaPesaje)
            throw new PesajeFueraDeOrdenException(fechaPesaje, ultimoPesaje.FechaPesaje);

        var pesaje = Pesaje.Crear(Id, fechaPesaje, peso, observaciones);
        _pesajes.Add(pesaje);

        RaiseDomainEvent(new PesajeRegistradoEvent(Id, pesaje.Id, peso.Kilogramos, fechaPesaje));

        return pesaje;
    }

    /// <summary>
    /// Registra un evento sanitario sobre el animal.
    /// Solo animales activos pueden registrar eventos sanitarios.
    /// </summary>
    public EventoSanitario RegistrarEventoSanitario(
        DateOnly fechaEvento,
        string diagnostico,
        string descripcion,
        SeveridadEvento severidad,
        string? tratamiento = null)
    {
        ValidarAnimalActivo();

        var evento = EventoSanitario.Crear(
            Id, fechaEvento, diagnostico, descripcion, severidad, tratamiento);

        _eventosSanitarios.Add(evento);

        // Si el evento es grave, actualizar estado sanitario.
        if (severidad >= SeveridadEvento.Grave)
            EstadoSanitario = EstadoSanitario.EnTratamiento;

        RaiseDomainEvent(new EventoSanitarioRegistradoEvent(
            Id, evento.Id, diagnostico, (int)severidad, fechaEvento));

        return evento;
    }

    /// <summary>
    /// Marca el animal como vendido. Operación irreversible.
    /// </summary>
    public void MarcarComoVendido()
    {
        ValidarAnimalActivo();
        EstadoProductivo = EstadoProductivo.Vendido;
    }

    /// <summary>
    /// Registra la muerte del animal. Operación irreversible.
    /// </summary>
    public void RegistrarMuerte()
    {
        if (EstadoProductivo == EstadoProductivo.Muerto)
            throw new DomainException("El animal ya está registrado como muerto.");

        EstadoProductivo = EstadoProductivo.Muerto;
    }

    // --- Queries sobre el estado del animal ---

    public bool EstaActivo => EstadoProductivo == EstadoProductivo.EnEngorde;

    /// <summary>Retorna el último pesaje registrado, o null si no hay pesajes.</summary>
    public Pesaje? UltimoPesaje => _pesajes
        .OrderByDescending(p => p.FechaPesaje)
        .FirstOrDefault();

    /// <summary>Calcula el peso actual estimado (el del último pesaje, o el de ingreso).</summary>
    public Peso PesoActual => UltimoPesaje?.Peso ?? PesoIngreso;

    /// <summary>Días transcurridos desde el ingreso al feedlot.</summary>
    public int DiasEnEngorde => DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - FechaIngreso.DayNumber;


    // --- Validación de invariante central ---

    private void ValidarAnimalActivo()
    {
        if (!EstaActivo)
            throw new AnimalInactivoException(Id);
    }
}
