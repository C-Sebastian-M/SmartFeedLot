namespace Feedlot.Domain.Enums;

/// <summary>
/// Tipo comercial del animal según la clasificación de subasta (SUBAGAN).
/// Permite emparejar cada animal con el precio/kg correcto de la subasta,
/// que viene segmentado por estos mismos códigos.
/// </summary>
public enum TipoComercial
{
    /// <summary>Macho de Ceba.</summary>
    MC = 1,

    /// <summary>Macho de Levante.</summary>
    ML = 2,

    /// <summary>Hembra de Vientre.</summary>
    HV = 3,

    /// <summary>Hembra de Levante.</summary>
    HL = 4,

    /// <summary>Vaca Escotera.</summary>
    VE = 5,

    /// <summary>Vaca de Cría.</summary>
    VC = 6,

    /// <summary>Toro.</summary>
    TO = 7
}
