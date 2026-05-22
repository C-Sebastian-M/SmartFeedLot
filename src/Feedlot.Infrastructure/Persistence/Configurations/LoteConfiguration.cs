using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración del Aggregate Lote.
/// 
/// CORRECCIÓN: Capacidad es un Value Object con dos campos (Maxima, Actual).
/// EF Core no puede mapear un VO con múltiples campos a una sola propiedad
/// con HasConversion. Se mapean como dos shadow properties independientes
/// y el repositorio hidrata el VO al cargar desde BD.
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

        // Capacidad Value Object → dos shadow properties independientes.
        // El repositorio usa estas columnas para reconstruir el VO al cargar.
        builder.Property(l => l.Capacidad)
            .HasColumnName("capacidad_maxima")
            .IsRequired()
            .HasConversion(
                c => c.Maxima,
                maxima => Capacidad.Crear(maxima, 0));

        // Shadow property para la cantidad actual de animales.
        builder.Property<int>("animales_actuales")
            .HasColumnName("animales_actuales")
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasMany(l => l.AnimalesLote)
            .WithOne()
            .HasForeignKey(al => al.LoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.AnimalesLote)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
