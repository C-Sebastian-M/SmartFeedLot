namespace Feedlot.Domain.Enums;

/// <summary>
/// Estado sanitario del animal. Afecta restricciones de movimiento y venta.
/// </summary>
public enum EstadoSanitario
{
    /// <summary>Animal sin observaciones sanitarias.</summary>
    Sano = 1,

    /// <summary>Animal bajo tratamiento activo.</summary>
    EnTratamiento = 2,

    /// <summary>Animal en período de retiro de medicamento.</summary>
    EnRetiro = 3,

    /// <summary>Animal con restricción sanitaria (no apto para venta).</summary>
    Restringido = 4
}
