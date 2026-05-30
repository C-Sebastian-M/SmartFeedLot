using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class AporteSocioConfiguration : IEntityTypeConfiguration<AporteSocio>
{
    public void Configure(EntityTypeBuilder<AporteSocio> builder)
    {
        builder.ToTable("AportesSocios");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.SocioId).IsRequired();
        builder.Property(a => a.ItemInversionId).IsRequired();

        builder.OwnsOne(a => a.Monto, dinero =>
        {
            dinero.Property(d => d.Monto).HasColumnName("Monto").HasColumnType("decimal(18,2)").IsRequired();
            dinero.Property(d => d.Moneda).HasColumnName("Moneda").HasMaxLength(3).IsRequired();
        });

        builder.HasIndex(a => new { a.SocioId, a.ItemInversionId }).IsUnique();
    }
}
