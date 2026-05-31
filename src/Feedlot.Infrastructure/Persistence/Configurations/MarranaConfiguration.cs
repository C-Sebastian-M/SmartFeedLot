using Feedlot.Domain.Entities;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class MarranaConfiguration : IEntityTypeConfiguration<Marrana>
{
    public void Configure(EntityTypeBuilder<Marrana> builder)
    {
        builder.ToTable("marranas");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(m => m.Identificacion)
            .HasColumnName("identificacion")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.FechaCompra)
            .HasColumnName("fecha_compra")
            .IsRequired();

        builder.Property(m => m.Costo)
            .HasColumnName("costo")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(d => d.Monto, m => Dinero.Crear(m, "COP"));

        builder.Property<string>("costo_moneda")
            .HasColumnName("costo_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.HasMany(m => m.Camadas)
            .WithOne()
            .HasForeignKey(c => c.MarranaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.Identificacion).IsUnique().HasDatabaseName("ux_marranas_identificacion");
    }
}
