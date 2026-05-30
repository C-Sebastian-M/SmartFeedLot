using Feedlot.Domain.Entities;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class PrestamoConfiguration : IEntityTypeConfiguration<Prestamo>
{
    public void Configure(EntityTypeBuilder<Prestamo> builder)
    {
        builder.ToTable("prestamos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(p => p.Capital)
            .HasColumnName("capital")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("capital_moneda")
            .HasColumnName("capital_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.Property(p => p.TasaMensual)
            .HasColumnName("tasa_mensual")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(p => p.NCuotas)
            .HasColumnName("n_cuotas")
            .IsRequired();

        builder.Property(p => p.FechaInicio)
            .HasColumnName("fecha_inicio")
            .IsRequired();

        builder.Property(p => p.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasMany(p => p.Cuotas)
            .WithOne()
            .HasForeignKey(c => c.PrestamoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Access private backing field for collection navigation if EF needs it
        builder.Navigation(p => p.Cuotas)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
