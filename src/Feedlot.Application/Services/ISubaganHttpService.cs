namespace Feedlot.Application.Services;

public sealed record SubaganLoteData(
    int LoteId, int NumeroLote, string CodigoTipo, string DescripcionTipo,
    int Cantidad, decimal PesoTotal, decimal PesoProm, decimal PrecioPorKg,
    string Procedencia, string? Observaciones, DateOnly Fecha);

/// <summary>
/// Un evento de subasta tal como aparece en el calendario de SUBAGAN.
/// </summary>
public sealed record SubaganEventoCalendarioData(
    int EventId,
    string Titulo,
    DateOnly Fecha);

public interface ISubaganHttpService
{
    Task<bool> LoginAsync(CancellationToken ct = default);
    Task<List<SubaganLoteData>> ObtenerLotesAsync(int eventId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene la lista de eventos del calendario de SUBAGAN (pasados, presentes y futuros).
    /// </summary>
    Task<List<SubaganEventoCalendarioData>> ObtenerEventosCalendarioAsync(CancellationToken ct = default);
}
