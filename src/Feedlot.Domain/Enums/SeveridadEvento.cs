namespace Feedlot.Domain.Enums;

/// <summary>Severidad de un evento sanitario. Determina alertas y restricciones.</summary>
public enum SeveridadEvento
{
    /// <summary>Evento menor, solo informativo.</summary>
    Leve = 1,

    /// <summary>Requiere seguimiento activo.</summary>
    Moderado = 2,

    /// <summary>Requiere aislamiento y tratamiento intensivo.</summary>
    Grave = 3,

    /// <summary>Riesgo de muerte. Alerta inmediata.</summary>
    Critico = 4
}
