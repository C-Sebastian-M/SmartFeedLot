using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class AnimalLoteConfiguration : IEntityTypeConfiguration<AnimalLote>
{
    public void Configure(EntityTypeBuilder<AnimalLote> builder)
    {
        builder.ToTable("animal_lotes");

        builder.HasKey(al => al.Id);
        builder.Property(al => al.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(al => al.LoteId)
            .HasColumnName("lote_id")
            .IsRequired();

        builder.Property(al => al.AnimalId)
            .HasColumnName("animal_id")
            .IsRequired();

        builder.Property(al => al.FechaIngreso)
            .HasColumnName("fecha_ingreso")
            .IsRequired();

        builder.Property(al => al.FechaEgreso)
            .HasColumnName("fecha_egreso");

        builder.Property(al => al.MotivoIngreso)
            .HasColumnName("motivo_ingreso")
            .HasMaxLength(30)
            .IsRequired()
            .HasConversion(
                m => m.ToString(),
                m => Enum.Parse<MotivoMovimiento>(m));

        builder.Property(al => al.MotivoEgreso)
            .HasColumnName("motivo_egreso")
            .HasMaxLength(30)
            .HasConversion(
                m => m.HasValue ? m.Value.ToString() : null,
                s => s != null ? Enum.Parse<MotivoMovimiento>(s) : (MotivoMovimiento?)null);

        builder.Property(al => al.EsActivo)
            .HasColumnName("es_activo")
            .IsRequired();

        // Índice crítico: buscar el lote activo de un animal es la consulta
        // más frecuente del sistema (se ejecuta en cada movimiento).
        builder.HasIndex(al => new { al.AnimalId, al.EsActivo })
            .HasDatabaseName("ix_animal_lotes_animal_activo");

        builder.HasIndex(al => new { al.LoteId, al.EsActivo })
            .HasDatabaseName("ix_animal_lotes_lote_activo");
    }
}
