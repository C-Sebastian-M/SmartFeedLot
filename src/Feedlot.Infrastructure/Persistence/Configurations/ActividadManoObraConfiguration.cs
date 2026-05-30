using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class ActividadManoObraConfiguration : IEntityTypeConfiguration<ActividadManoObra>
{
    public void Configure(EntityTypeBuilder<ActividadManoObra> builder)
    {
        builder.ToTable("ActividadesManoObra");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EmpleadoId).IsRequired();
        builder.Property(a => a.Tipo).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Fecha).IsRequired();

        builder.OwnsOne(a => a.Costo, dinero =>
        {
            dinero.Property(d => d.Monto).HasColumnName("CostoMonto").HasColumnType("decimal(18,2)").IsRequired();
            dinero.Property(d => d.Moneda).HasColumnName("CostoMoneda").HasMaxLength(3).IsRequired();
        });
    }
}
