using Feedlot.Domain.Entities;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class PresupuestoConfiguration : IEntityTypeConfiguration<Presupuesto>
{
    public void Configure(EntityTypeBuilder<Presupuesto> builder)
    {
        builder.ToTable("presupuestos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(p => p.PeriodoAnio)
            .HasColumnName("periodo_anio")
            .IsRequired();

        builder.Property(p => p.PeriodoMes)
            .HasColumnName("periodo_mes")
            .IsRequired();

        builder.Property(p => p.CategoriaGastoId)
            .HasColumnName("categoria_gasto_id")
            .IsRequired();

        builder.Property(p => p.MontoPresupuestado)
            .HasColumnName("monto_presupuestado")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                m => Dinero.Crear(m, "COP"));

        builder.Property<string>("monto_moneda")
            .HasColumnName("monto_moneda")
            .HasMaxLength(3)
            .HasDefaultValue("COP");

        builder.Property(p => p.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(500);

        // Relación con CategoriaGasto
        builder.HasOne(p => p.CategoriaGasto)
            .WithMany()
            .HasForeignKey(p => p.CategoriaGastoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unicidad: una sola línea de presupuesto por categoría/período
        builder.HasIndex(p => new { p.PeriodoAnio, p.PeriodoMes, p.CategoriaGastoId })
            .IsUnique()
            .HasDatabaseName("ix_presupuestos_periodo_categoria");
    }
}
