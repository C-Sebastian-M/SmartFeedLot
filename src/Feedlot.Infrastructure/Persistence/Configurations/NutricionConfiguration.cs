using Feedlot.Domain.Entities;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class IngredienteConfiguration : IEntityTypeConfiguration<Ingrediente>
{
    public void Configure(EntityTypeBuilder<Ingrediente> builder)
    {
        builder.ToTable("ingredientes");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(i => i.Nombre)
            .IsUnique()
            .HasDatabaseName("ix_ingredientes_nombre");

        builder.Property(i => i.CostoKg)
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

        builder.Property(i => i.ProteinaPct)
            .HasColumnName("proteina_pct")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(i => i.UnidadMedida)
            .HasColumnName("unidad_medida")
            .HasMaxLength(20)
            .IsRequired();
    }
}

public sealed class RacionIngredienteConfiguration : IEntityTypeConfiguration<RacionIngrediente>
{
    public void Configure(EntityTypeBuilder<RacionIngrediente> builder)
    {
        builder.ToTable("racion_ingredientes");

        builder.HasKey(ri => ri.Id);
        builder.Property(ri => ri.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ri => ri.RacionId)
            .HasColumnName("racion_id")
            .IsRequired();

        builder.Property(ri => ri.IngredienteId)
            .HasColumnName("ingrediente_id")
            .IsRequired();

        builder.Property(ri => ri.ProporcionPct)
            .HasColumnName("proporcion_pct")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.HasIndex(ri => new { ri.RacionId, ri.IngredienteId })
            .IsUnique()
            .HasDatabaseName("ix_racion_ingredientes_unique");
    }
}
