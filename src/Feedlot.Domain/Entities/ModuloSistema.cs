using Feedlot.Domain.Common;

namespace Feedlot.Domain.Entities;

/// <summary>
/// Representa un módulo funcional del sistema (Animales, Finanzas, Porcino...).
/// El Admin puede activarlo o desactivarlo; los módulos desactivados se ocultan
/// del menú para todos los usuarios.
///
/// La <see cref="Clave"/> es un identificador estable (no cambia) que el frontend
/// usa para decidir qué entradas de menú mostrar.
/// </summary>
public sealed class ModuloSistema : Entity<Guid>
{
    private ModuloSistema() { }

    private ModuloSistema(Guid id, string clave, string nombre, bool activo, int orden)
        : base(id)
    {
        Clave = clave;
        Nombre = nombre;
        Activo = activo;
        Orden = orden;
    }

    /// <summary>Identificador estable del módulo (ej. "porcino", "finanzas").</summary>
    public string Clave { get; private set; } = null!;

    /// <summary>Nombre legible para mostrar en la configuración.</summary>
    public string Nombre { get; private set; } = null!;

    /// <summary>Si está visible/habilitado en el menú.</summary>
    public bool Activo { get; private set; }

    /// <summary>Orden de presentación en la pantalla de configuración.</summary>
    public int Orden { get; private set; }

    public static ModuloSistema Crear(string clave, string nombre, bool activo, int orden)
        => new(Guid.NewGuid(), clave.Trim().ToLowerInvariant(), nombre.Trim(), activo, orden);

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;
    public void EstablecerActivo(bool activo) => Activo = activo;
}
