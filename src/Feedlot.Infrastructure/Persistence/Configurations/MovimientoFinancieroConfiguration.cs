using Feedlot.Domain.Entities;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class MovimientoFinancieroConfiguration : IEntityTypeConfiguration<MovimientoFinanciero>
{
    public void Configure(EntityTypeBuilder<MovimientoFinanciero> builder)
    {
        builder.ToTable("movimientos_financieros");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(m => m.Fecha)
            .HasColumnName("fecha")
            .IsRequired();

        builder.Property(m => m.PeriodoAnio)
            .HasColumnName("periodo_anio")
            .IsRequired();

        builder.Property(m => m.PeriodoMes)
            .HasColumnName("periodo_mes")
            .IsRequired();

        builder.Property(m => m.CategoriaGastoId)
            .HasColumnName("categoria_gasto_id")
            .IsRequired();

        builder.Property(m => m.Monto)
            .HasColumnName("monto")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("monto_moneda")
            .HasColumnName("monto_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.Property(m => m.Origen)
            .HasColumnName("origen")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.SocioId)
            .HasColumnName("socio_id");

        builder.Property(m => m.RegistradoPorId)
            .HasColumnName("registrado_por_id")
            .IsRequired();

        // Relaciones
        builder.HasOne(m => m.CategoriaGasto)
            .WithMany()
            .HasForeignKey(m => m.CategoriaGastoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Socio)
            .WithMany()
            .HasForeignKey(m => m.SocioId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices
        builder.HasIndex(m => new { m.PeriodoAnio, m.PeriodoMes })
            .HasDatabaseName("ix_movimientos_financieros_periodo");

        builder.HasIndex(m => m.Origen)
            .HasDatabaseName("ix_movimientos_financieros_origen");
    }
}
