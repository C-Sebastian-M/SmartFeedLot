using Feedlot.Domain.Common;

namespace Feedlot.Domain.Entities;

public sealed class SubaganEvento : AggregateRoot<Guid>
{
    private readonly List<SubaganLote> _lotes = new();

    private SubaganEvento() { }

    private SubaganEvento(Guid id, int subaganEventoId, int? numeroSubasta,
        DateOnly fecha, string sede) : base(id)
    {
        SubaganEventoId = subaganEventoId;
        NumeroSubasta   = numeroSubasta;
        Fecha           = fecha;
        Sede            = sede;
        ImportadoEn     = DateTime.UtcNow;
    }

    public int SubaganEventoId { get; private set; }
    public int? NumeroSubasta  { get; private set; }
    public DateOnly Fecha      { get; private set; }
    public string Sede         { get; private set; } = null!;
    public DateTime ImportadoEn { get; private set; }

    public IReadOnlyList<SubaganLote> Lotes => _lotes.AsReadOnly();

    public static SubaganEvento Crear(int subaganEventoId, int? numeroSubasta, DateOnly fecha, string sede)
        => new(Guid.NewGuid(), subaganEventoId, numeroSubasta, fecha, sede.Trim());

    public void AgregarLote(int loteId, int numeroLote, string codigoTipo, string descripcionTipo,
        int cantidad, decimal pesoTotal, decimal pesoProm, decimal precioPorKg,
        string procedencia, string? observaciones)
    {
        _lotes.Add(SubaganLote.Crear(Id, loteId, numeroLote, codigoTipo, descripcionTipo,
            cantidad, pesoTotal, pesoProm, precioPorKg, procedencia, observaciones, Fecha));
    }
}
