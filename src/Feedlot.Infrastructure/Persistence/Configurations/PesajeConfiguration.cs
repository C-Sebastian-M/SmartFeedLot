using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class PesajeConfiguration : IEntityTypeConfiguration<Pesaje>
{
    public void Configure(EntityTypeBuilder<Pesaje> builder)
    {
        builder.ToTable("pesajes");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.AnimalId)
            .HasColumnName("animal_id")
            .IsRequired();

        builder.Property(p => p.FechaPesaje)
            .HasColumnName("fecha_pesaje")
            .IsRequired();

        builder.Property(p => p.Peso)
            .HasColumnName("peso_kg")
            .HasPrecision(10, 3)
            .IsRequired()
            .HasConversion(
                p => p.Kilogramos,
                kg => Peso.Crear(kg));

        builder.Property(p => p.Observaciones)
            .HasColumnName("observaciones")
            .HasMaxLength(500);

        // Índice compuesto para garantizar orden cronológico y búsquedas rápidas por animal + fecha.
        builder.HasIndex(p => new { p.AnimalId, p.FechaPesaje })
            .HasDatabaseName("ix_pesajes_animal_fecha");
    }
}
