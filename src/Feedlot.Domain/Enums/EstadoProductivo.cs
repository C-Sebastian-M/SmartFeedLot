namespace Feedlot.Domain.Enums;

/// <summary>
/// Estado productivo del animal dentro del sistema de feedlot.
/// Controla qué operaciones están permitidas sobre el animal.
/// </summary>
public enum EstadoProductivo
{
    /// <summary>Animal activo en proceso de engorde.</summary>
    EnEngorde = 1,

    /// <summary>Animal vendido. No admite nuevos eventos.</summary>
    Vendido = 2,

    /// <summary>Animal muerto. No admite nuevos eventos.</summary>
    Muerto = 3,

    /// <summary>Animal retirado del sistema por otra causa.</summary>
    Retirado = 4
}
