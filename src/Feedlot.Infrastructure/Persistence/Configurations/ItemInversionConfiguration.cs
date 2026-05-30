using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class ItemInversionConfiguration : IEntityTypeConfiguration<ItemInversion>
{
    public void Configure(EntityTypeBuilder<ItemInversion> builder)
    {
        builder.ToTable("ItemsInversion");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.EtapaInversionId).IsRequired();
        builder.Property(i => i.Producto).IsRequired().HasMaxLength(300);
        builder.Property(i => i.Observacion).HasMaxLength(500);

        builder.OwnsOne(i => i.Costo, dinero =>
        {
            dinero.Property(d => d.Monto).HasColumnName("CostoMonto").HasColumnType("decimal(18,2)").IsRequired();
            dinero.Property(d => d.Moneda).HasColumnName("CostoMoneda").HasMaxLength(3).IsRequired();
        });

        builder.Property(i => i.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.PorcentajeAvance)
            .HasColumnType("decimal(5,2)")
            .IsRequired();
    }
}
