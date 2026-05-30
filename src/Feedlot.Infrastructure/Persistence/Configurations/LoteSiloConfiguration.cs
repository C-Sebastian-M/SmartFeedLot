using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class LoteSiloConfiguration : IEntityTypeConfiguration<LoteSilo>
{
    public void Configure(EntityTypeBuilder<LoteSilo> builder)
    {
        builder.ToTable("LotesSilo");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.CorteCaniaId);
        builder.Property(l => l.FechaProduccion).IsRequired();
        builder.Property(l => l.Bolsas).IsRequired();
        builder.Property(l => l.Observacion).HasMaxLength(500);

        builder.OwnsOne(l => l.CostoUnitario, dinero =>
        {
            dinero.Property(d => d.Monto).HasColumnName("CostoUnitarioMonto").HasColumnType("decimal(18,2)").IsRequired();
            dinero.Property(d => d.Moneda).HasColumnName("CostoUnitarioMoneda").HasMaxLength(3).IsRequired();
        });
    }
}
