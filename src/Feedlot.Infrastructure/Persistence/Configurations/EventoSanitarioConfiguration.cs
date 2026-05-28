using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class EventoSanitarioConfiguration : IEntityTypeConfiguration<EventoSanitario>
{
    public void Configure(EntityTypeBuilder<EventoSanitario> builder)
    {
        builder.ToTable("eventos_sanitarios");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.AnimalId)
            .HasColumnName("animal_id")
            .IsRequired();

        builder.Property(e => e.FechaEvento)
            .HasColumnName("fecha_evento")
            .IsRequired();

        builder.Property(e => e.Diagnostico)
            .HasColumnName("diagnostico")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.Severidad)
            .HasColumnName("severidad")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                s => s.ToString(),
                s => Enum.Parse<SeveridadEvento>(s));

        builder.Property(e => e.Tratamiento)
            .HasColumnName("tratamiento")
            .HasMaxLength(500);

        builder.Property(e => e.TipoEvento)
            .HasColumnName("tipo_evento")
            .HasMaxLength(20);

        builder.Property(e => e.ProximaDosis)
            .HasColumnName("proxima_dosis");

        builder.Property(e => e.Responsable)
            .HasColumnName("responsable")
            .HasMaxLength(200);

        builder.HasIndex(e => new { e.AnimalId, e.FechaEvento })
            .HasDatabaseName("ix_eventos_sanitarios_animal_fecha");

        builder.HasIndex(e => e.ProximaDosis)
            .HasDatabaseName("ix_eventos_sanitarios_proxima_dosis")
            .HasFilter("\"proxima_dosis\" IS NOT NULL");

        builder.HasIndex(e => e.Severidad)
            .HasDatabaseName("ix_eventos_sanitarios_severidad");
    }
}
