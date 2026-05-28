using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class VentaItemConfiguration : IEntityTypeConfiguration<VentaItem>
{
    public void Configure(EntityTypeBuilder<VentaItem> builder)
    {
        builder.ToTable("venta_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(i => i.VentaId).HasColumnName("venta_id").IsRequired();
        builder.Property(i => i.AnimalId).HasColumnName("animal_id").IsRequired();
        builder.Property(i => i.PrecioVenta).HasColumnName("precio_venta").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.PesoVentaKg).HasColumnName("peso_venta_kg").HasColumnType("decimal(10,2)").IsRequired();

        builder.HasIndex(i => i.VentaId).HasDatabaseName("ix_venta_items_venta");
        builder.HasIndex(i => i.AnimalId).HasDatabaseName("ix_venta_items_animal");
    }
}
