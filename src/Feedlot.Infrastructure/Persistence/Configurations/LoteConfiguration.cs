using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración del Aggregate Lote.
/// El Value Object Capacidad se desnormaliza en dos columnas:
/// capacidad_maxima y animales_actuales — optimiza las consultas de ocupación
/// sin necesidad de COUNT() en tiempo real.
/// </summary>
public sealed class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("lotes");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(l => l.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(l => l.Codigo)
            .IsUnique()
            .HasDatabaseName("ix_lotes_codigo");

        builder.Property(l => l.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.Estado)
            .HasColumnName("estado")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                e => e.ToString(),
                e => Enum.Parse<EstadoLote>(e));

        builder.HasIndex(l => l.Estado)
            .HasDatabaseName("ix_lotes_estado");

        // Value Object Capacidad → dos columnas separadas.
        // EF Core no puede mapear un VO con constructor privado directamente
        // a columnas múltiples sin conversión manual, así que usamos
        // HasConversion + shadow properties para las dos columnas.
        builder.Property(l => l.Capacidad)
            .HasColumnName("capacidad_maxima")
            .IsRequired()
            .HasConversion(
                c => c.Maxima,
                maxima => Capacidad.Crear(maxima, 0)); // El Actual se hidrata en el repositorio.

        // Columna shadow para animales_actuales — EF Core la gestiona
        // pero no está en la entidad directamente (está dentro del VO Capacidad).
        builder.Property<int>("animales_actuales")
            .HasColumnName("animales_actuales")
            .HasDefaultValue(0)
            .IsRequired();

        // Navigation a AnimalLote — colección interna del aggregate.
        builder.HasMany(l => l.AnimalesLote)
            .WithOne()
            .HasForeignKey(al => al.LoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.AnimalesLote)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
