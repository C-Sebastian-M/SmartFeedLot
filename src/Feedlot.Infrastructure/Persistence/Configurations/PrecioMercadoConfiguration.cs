using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class PrecioMercadoConfiguration : IEntityTypeConfiguration<PrecioMercado>
{
    public void Configure(EntityTypeBuilder<PrecioMercado> builder)
    {
        builder.ToTable("precios_mercado");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(p => p.Fecha)
            .HasColumnName("fecha")
            .IsRequired();

        builder.Property(p => p.Especie)
            .HasColumnName("especie")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.PrecioPorKg)
            .HasColumnName("precio_por_kg")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Fuente)
            .HasColumnName("fuente")
            .HasMaxLength(200)
            .IsRequired();
    }
}
