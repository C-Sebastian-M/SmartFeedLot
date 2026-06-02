using Feedlot.Domain.Common;

namespace Feedlot.Domain.Entities;

public sealed class SubaganLote : Entity<Guid>
{
    private SubaganLote() { }

    private SubaganLote(Guid id, Guid eventoId, int loteId, int numeroLote,
        string codigoTipo, string descripcionTipo, int cantidad,
        decimal pesoTotal, decimal pesoProm, decimal precioPorKg,
        string procedencia, string? observaciones, DateOnly fecha) : base(id)
    {
        SubaganEventoId = eventoId;
        LoteId          = loteId;
        NumeroLote      = numeroLote;
        CodigoTipo      = codigoTipo.Trim();
        DescripcionTipo = descripcionTipo.Trim();
        Cantidad        = cantidad;
        PesoTotal       = pesoTotal;
        PesoProm        = pesoProm;
        PrecioPorKg     = precioPorKg;
        Procedencia     = procedencia.Trim();
        Observaciones   = string.IsNullOrWhiteSpace(observaciones) || observaciones == "---" ? null : observaciones.Trim();
        Fecha           = fecha;
    }

    public Guid SubaganEventoId  { get; private set; }
    public int LoteId            { get; private set; }
    public int NumeroLote        { get; private set; }
    public string CodigoTipo     { get; private set; } = null!;
    public string DescripcionTipo { get; private set; } = null!;
    public int Cantidad          { get; private set; }
    public decimal PesoTotal     { get; private set; }
    public decimal PesoProm      { get; private set; }
    public decimal PrecioPorKg   { get; private set; }
    public string Procedencia    { get; private set; } = null!;
    public string? Observaciones { get; private set; }
    public DateOnly Fecha        { get; private set; }

    internal static SubaganLote Crear(Guid eventoId, int loteId, int numeroLote,
        string codigoTipo, string descripcionTipo, int cantidad,
        decimal pesoTotal, decimal pesoProm, decimal precioPorKg,
        string procedencia, string? observaciones, DateOnly fecha)
        => new(Guid.NewGuid(), eventoId, loteId, numeroLote, codigoTipo, descripcionTipo,
               cantidad, pesoTotal, pesoProm, precioPorKg, procedencia, observaciones, fecha);
}
