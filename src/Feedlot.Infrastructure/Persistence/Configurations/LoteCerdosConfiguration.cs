using Feedlot.Domain.Entities;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class LoteCerdosConfiguration : IEntityTypeConfiguration<LoteCerdos>
{
    public void Configure(EntityTypeBuilder<LoteCerdos> builder)
    {
        builder.ToTable("lotes_cerdos");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(l => l.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.FechaInicio)
            .HasColumnName("fecha_inicio")
            .IsRequired();

        builder.Property(l => l.NAnimales)
            .HasColumnName("n_animales")
            .IsRequired();

        builder.Property(l => l.PesoPromedioKg)
            .HasColumnName("peso_promedio_kg")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(l => l.Ciclo)
            .HasColumnName("ciclo")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.CamadaId)
            .HasColumnName("camada_id");

        builder.Property(l => l.PrecioVentaKg)
            .HasColumnName("precio_venta_kg")
            .HasPrecision(18, 2)
            .HasConversion(d => d != null ? d.Monto : (decimal?)null,
                m => m.HasValue ? Dinero.Crear(m.Value, "COP") : null);

        builder.Property<string?>("precio_venta_moneda")
            .HasColumnName("precio_venta_moneda")
            .HasMaxLength(3);

        builder.Property(l => l.FechaVenta)
            .HasColumnName("fecha_venta");

        builder.HasIndex(l => l.Codigo).IsUnique().HasDatabaseName("ux_lotes_cerdos_codigo");
    }
}
