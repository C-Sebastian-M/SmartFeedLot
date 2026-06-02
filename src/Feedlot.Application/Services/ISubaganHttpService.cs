namespace Feedlot.Application.Services;

public sealed record SubaganLoteData(
    int LoteId, int NumeroLote, string CodigoTipo, string DescripcionTipo,
    int Cantidad, decimal PesoTotal, decimal PesoProm, decimal PrecioPorKg,
    string Procedencia, string? Observaciones, DateOnly Fecha);

public interface ISubaganHttpService
{
    Task<bool> LoginAsync(CancellationToken ct = default);
    Task<List<SubaganLoteData>> ObtenerLotesAsync(int eventId, CancellationToken ct = default);
}
