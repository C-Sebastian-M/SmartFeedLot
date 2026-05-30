using Feedlot.Domain.Common;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class CuotaAmortizacion : Entity<Guid>
{
    private CuotaAmortizacion() { }

    internal CuotaAmortizacion(
        Guid id,
        Guid prestamoId,
        int numeroCuota,
        DateOnly fechaVencimiento,
        Dinero cuota,
        Dinero interes,
        Dinero abonoCapital,
        Dinero saldoPendiente) : base(id)
    {
        PrestamoId = prestamoId;
        NumeroCuota = numeroCuota;
        FechaVencimiento = fechaVencimiento;
        Cuota = cuota;
        Interes = interes;
        AbonoCapital = abonoCapital;
        SaldoPendiente = saldoPendiente;
        Pagada = false;
    }

    public Guid PrestamoId { get; private set; }
    public int NumeroCuota { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public Dinero Cuota { get; private set; } = null!;
    public Dinero Interes { get; private set; } = null!;
    public Dinero AbonoCapital { get; private set; } = null!;
    public Dinero SaldoPendiente { get; private set; } = null!;
    public bool Pagada { get; private set; }
    public DateOnly? FechaPago { get; private set; }

    public void RegistrarPago(DateOnly fechaPago)
    {
        Pagada = true;
        FechaPago = fechaPago;
    }

    public void AnularPago()
    {
        Pagada = false;
        FechaPago = null;
    }
}
