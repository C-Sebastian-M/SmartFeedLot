using Feedlot.Domain.Entities;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class ConsumoAlimenticioConfiguration : IEntityTypeConfiguration<ConsumoAlimenticio>
{
    public void Configure(EntityTypeBuilder<ConsumoAlimenticio> builder)
    {
        builder.ToTable("consumos_alimenticios");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.LoteId)
            .HasColumnName("lote_id")
            .IsRequired();

        builder.Property(c => c.RacionId)
            .HasColumnName("racion_id")
            .IsRequired();

        builder.Property(c => c.Fecha)
            .HasColumnName("fecha")
            .IsRequired();

        builder.Property(c => c.CantidadKg)
            .HasColumnName("cantidad_kg")
            .HasPrecision(12, 3)
            .IsRequired()
            .HasConversion(
                ck => ck.Valor,
                v => CantidadKilogramos.Crear(v));

        builder.Property(c => c.CostoTotal)
            .HasColumnName("costo_total")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("costo_moneda")
            .HasColumnName("costo_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.Property(c => c.RegistradoPorId)
            .HasColumnName("registrado_por_id")
            .IsRequired();

        // Índice para agregaciones por lote en un período — usado intensivamente
        // por las queries analíticas de ICA y costo por kg.
        builder.HasIndex(c => new { c.LoteId, c.Fecha })
            .HasDatabaseName("ix_consumos_lote_fecha");
    }
}
