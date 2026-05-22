using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

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

        // CapacidadMaxima es ahora una propiedad pública con setter privado.
        // EF Core la mapea directamente sin necesidad de backing fields ni reflexión.
        builder.Property(l => l.CapacidadMaxima)
            .HasColumnName("capacidad_maxima")
            .IsRequired();

        // Capacidad es una propiedad calculada — no tiene columna propia.
        builder.Ignore(l => l.Capacidad);

        builder.HasMany(l => l.AnimalesLote)
            .WithOne()
            .HasForeignKey(al => al.LoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.AnimalesLote)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
