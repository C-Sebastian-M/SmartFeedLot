namespace Feedlot.Domain.Enums;

/// <summary>Estado operativo de un lote de engorde.</summary>
public enum EstadoLote
{
    /// <summary>Lote activo recibiendo animales o en proceso de engorde.</summary>
    Activo = 1,

    /// <summary>Lote cerrado. Ya fue liquidado o vaciado.</summary>
    Cerrado = 2,

    /// <summary>Lote en preparación. Aún no tiene animales.</summary>
    EnPreparacion = 3
}
