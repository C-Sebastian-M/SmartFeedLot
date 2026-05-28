using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("ventas");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(v => v.CompradorId).HasColumnName("comprador_id").IsRequired();
        builder.Property(v => v.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(v => v.Moneda).HasColumnName("moneda").HasMaxLength(5).IsRequired();
        builder.Property(v => v.Descripcion).HasColumnName("descripcion").HasMaxLength(500);

        builder.Ignore(v => v.MontoTotal);

        builder.HasMany(v => v.Items)
            .WithOne()
            .HasForeignKey(i => i.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.CompradorId).HasDatabaseName("ix_ventas_comprador");
        builder.HasIndex(v => v.Fecha).HasDatabaseName("ix_ventas_fecha");
    }
}
