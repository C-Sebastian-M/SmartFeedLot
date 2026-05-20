using Feedlot.Domain.Entities;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class RacionConfiguration : IEntityTypeConfiguration<Racion>
{
    public void Configure(EntityTypeBuilder<Racion> builder)
    {
        builder.ToTable("raciones");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(r => r.Nombre)
            .IsUnique()
            .HasDatabaseName("ix_raciones_nombre");

        builder.Property(r => r.CostoKg)
            .HasColumnName("costo_kg")
            .HasPrecision(12, 4)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("costo_moneda")
            .HasColumnName("costo_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.Property(r => r.ProteinaPct)
            .HasColumnName("proteina_pct")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.EnergiaMcal)
            .HasColumnName("energia_mcal")
            .HasPrecision(8, 4)
            .IsRequired();

        builder.Property(r => r.Activa)
            .HasColumnName("activa")
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(r => r.Ingredientes)
            .WithOne()
            .HasForeignKey(ri => ri.RacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Ingredientes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
