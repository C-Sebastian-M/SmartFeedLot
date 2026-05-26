namespace Feedlot.Domain.Enums;

/// <summary>
/// Categoría del costo operativo según la clasificación contable del Excel:
/// Materia Prima (alimento), Mano de Obra directa, y CIF (Costos Indirectos de Fabricación).
/// </summary>
public enum CategoriaCosto
{
    /// <summary>Mano de obra directa: alimentación, fumigación, mantenimiento, etc.</summary>
    ManoDeObra = 1,

    /// <summary>
    /// Costos Indirectos de Fabricación: gasolina, fertilizantes, alquiler de potrero,
    /// insumos de terreno que no son alimento directo del animal.
    /// </summary>
    CIF = 2
}
