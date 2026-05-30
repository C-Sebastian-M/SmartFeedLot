using Feedlot.Domain.Entities;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class CuotaAmortizacionConfiguration : IEntityTypeConfiguration<CuotaAmortizacion>
{
    public void Configure(EntityTypeBuilder<CuotaAmortizacion> builder)
    {
        builder.ToTable("cuotas_amortizacion");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(c => c.PrestamoId)
            .HasColumnName("prestamo_id")
            .IsRequired();

        builder.Property(c => c.NumeroCuota)
            .HasColumnName("numero_cuota")
            .IsRequired();

        builder.Property(c => c.FechaVencimiento)
            .HasColumnName("fecha_vencimiento")
            .IsRequired();

        builder.Property(c => c.Cuota)
            .HasColumnName("cuota")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("cuota_moneda")
            .HasColumnName("cuota_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.Property(c => c.Interes)
            .HasColumnName("interes")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("interes_moneda")
            .HasColumnName("interes_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.Property(c => c.AbonoCapital)
            .HasColumnName("abono_capital")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("abono_moneda")
            .HasColumnName("abono_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.Property(c => c.SaldoPendiente)
            .HasColumnName("saldo_pendiente")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("saldo_moneda")
            .HasColumnName("saldo_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.Property(c => c.Pagada)
            .HasColumnName("pagada")
            .IsRequired();

        builder.Property(c => c.FechaPago)
            .HasColumnName("fecha_pago");

        builder.HasIndex(c => new { c.PrestamoId, c.NumeroCuota })
            .IsUnique()
            .HasDatabaseName("ux_cuotas_amortizacion_prestamo_cuota");
    }
}
