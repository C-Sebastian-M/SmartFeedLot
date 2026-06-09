namespace Feedlot.Domain.Enums;

/// <summary>
/// Categoría del costo operativo según la clasificación contable del Excel:
/// Mano de Obra directa y CIF (Costos Indirectos de Fabricación).
/// </summary>
public enum CategoriaCosto
{
    /// <summary>
    /// Mano de obra directa: suministrar alimentación, preparar silo,
    /// fumigación, mantenimiento de cercas, riego/bombeo.
    /// </summary>
    ManoDeObra = 1,

    /// <summary>
    /// Costos Indirectos de Fabricación: gasolina moto bomba, grama fin,
    /// cal agrícola, urea, alquiler de potrero.
    /// </summary>
    CIF = 2
}
