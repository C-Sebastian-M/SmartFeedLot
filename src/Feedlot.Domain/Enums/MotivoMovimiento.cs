namespace Feedlot.Domain.Enums;

/// <summary>Motivo por el cual un animal se mueve entre lotes.</summary>
public enum MotivoMovimiento
{
    /// <summary>Ingreso inicial al sistema.</summary>
    IngresoInicial = 1,

    /// <summary>Reclasificación productiva (por GMD, tamaño, etc.).</summary>
    Reclasificacion = 2,

    /// <summary>Aislamiento sanitario.</summary>
    Sanitario = 3,

    /// <summary>Traslado por capacidad del lote.</summary>
    Capacidad = 4,

    /// <summary>Salida para venta.</summary>
    Venta = 5,

    /// <summary>Muerte del animal.</summary>
    Muerte = 6,

    /// <summary>Otro motivo no clasificado.</summary>
    Otro = 7
}
