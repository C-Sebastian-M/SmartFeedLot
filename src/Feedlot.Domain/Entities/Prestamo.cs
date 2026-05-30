using Feedlot.Domain.Common;
using Feedlot.Domain.Exceptions;
using Feedlot.Domain.ValueObjects;

namespace Feedlot.Domain.Entities;

public sealed class Prestamo : AggregateRoot<Guid>
{
    private readonly List<CuotaAmortizacion> _cuotas = new();

    private Prestamo() { }

    private Prestamo(
        Guid id,
        Dinero capital,
        decimal tasaMensual,
        int nCuotas,
        DateOnly fechaInicio,
        string descripcion) : base(id)
    {
        Capital = capital;
        TasaMensual = tasaMensual;
        NCuotas = nCuotas;
        FechaInicio = fechaInicio;
        Descripcion = descripcion;
    }

    public Dinero Capital { get; private set; } = null!;
    public decimal TasaMensual { get; private set; }
    public int NCuotas { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public string Descripcion { get; private set; } = null!;
    public IReadOnlyCollection<CuotaAmortizacion> Cuotas => _cuotas.AsReadOnly();

    public static Prestamo Crear(
        Dinero capital,
        decimal tasaMensual,
        int nCuotas,
        DateOnly fechaInicio,
        string descripcion)
    {
        if (capital.Monto <= 0)
            throw new DomainException("El capital del préstamo debe ser mayor a cero.");

        if (tasaMensual < 0)
            throw new DomainException("La tasa de interés mensual no puede ser negativa.");

        if (nCuotas <= 0)
            throw new DomainException("El número de cuotas debe ser mayor a cero.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción del préstamo no puede estar vacía.");

        var prestamo = new Prestamo(
            Guid.NewGuid(),
            capital,
            tasaMensual,
            nCuotas,
            fechaInicio,
            descripcion.Trim());

        prestamo.GenerarTablaAmortizacion();
        return prestamo;
    }

    public void GenerarTablaAmortizacion()
    {
        _cuotas.Clear();

        decimal capitalInicial = Capital.Monto;
        string moneda = Capital.Moneda;

        if (TasaMensual == 0)
        {
            decimal cuotaSinInteres = Math.Round(capitalInicial / NCuotas, 2);
            decimal saldo = capitalInicial;

            for (int k = 1; k <= NCuotas; k++)
            {
                decimal abono = k == NCuotas ? saldo : cuotaSinInteres;
                saldo -= abono;
                var vencimiento = FechaInicio.AddMonths(k);
                _cuotas.Add(new CuotaAmortizacion(
                    Guid.NewGuid(),
                    Id,
                    k,
                    vencimiento,
                    Dinero.Crear(abono, moneda),
                    Dinero.Cero(moneda),
                    Dinero.Crear(abono, moneda),
                    Dinero.Crear(Math.Max(0, saldo), moneda)
                ));
            }
            return;
        }

        double i = (double)(TasaMensual / 100);
        double factor = Math.Pow(1 + i, -NCuotas);
        decimal montoCuota = (decimal)((double)capitalInicial * i / (1 - factor));
        montoCuota = Math.Round(montoCuota, 2);

        decimal saldoRestante = capitalInicial;

        for (int k = 1; k <= NCuotas; k++)
        {
            decimal interesCuota = Math.Round(saldoRestante * (decimal)i, 2);
            decimal abonoCapital = k == NCuotas ? saldoRestante : Math.Round(montoCuota - interesCuota, 2);
            decimal cuotaFinal = k == NCuotas ? (abonoCapital + interesCuota) : montoCuota;

            saldoRestante -= abonoCapital;

            var vencimiento = FechaInicio.AddMonths(k);
            _cuotas.Add(new CuotaAmortizacion(
                Guid.NewGuid(),
                Id,
                k,
                vencimiento,
                Dinero.Crear(cuotaFinal, moneda),
                Dinero.Crear(interesCuota, moneda),
                Dinero.Crear(abonoCapital, moneda),
                Dinero.Crear(Math.Max(0, saldoRestante), moneda)
            ));
        }
    }
}
