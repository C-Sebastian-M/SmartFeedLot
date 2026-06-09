using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class CostoOperativoConfiguration : IEntityTypeConfiguration<CostoOperativo>
{
    public void Configure(EntityTypeBuilder<CostoOperativo> builder)
    {
        builder.ToTable("costos_operativos");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.LoteId)
            .HasColumnName("lote_id")
            .IsRequired();

        builder.HasIndex(c => c.LoteId)
            .HasDatabaseName("ix_costos_operativos_lote_id");

        builder.Property(c => c.Categoria)
            .HasColumnName("categoria")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                e => e.ToString(),
                e => Enum.Parse<CategoriaCosto>(e));

        builder.HasIndex(c => c.Categoria)
            .HasDatabaseName("ix_costos_operativos_categoria");

        builder.Property(c => c.Concepto)
            .HasColumnName("concepto")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Fecha)
            .HasColumnName("fecha")
            .IsRequired();

        builder.HasIndex(c => c.Fecha)
            .HasDatabaseName("ix_costos_operativos_fecha");

        builder.Property(c => c.Monto)
            .HasColumnName("monto")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("monto_moneda")
            .HasColumnName("monto_moneda")
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("COP");

        builder.Property(c => c.Observaciones)
            .HasColumnName("observaciones")
            .HasMaxLength(500);

        builder.Property(c => c.RegistradoPorId)
            .HasColumnName("registrado_por_id")
            .IsRequired();
    }
}
