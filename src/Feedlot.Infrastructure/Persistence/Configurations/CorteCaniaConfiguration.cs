using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class CorteCaniaConfiguration : IEntityTypeConfiguration<CorteCania>
{
    public void Configure(EntityTypeBuilder<CorteCania> builder)
    {
        builder.ToTable("CortesCania");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CultivoCaniaId).IsRequired();
        builder.Property(c => c.Fecha).IsRequired();
        builder.Property(c => c.NCalles).IsRequired();
        builder.Property(c => c.Horas).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(c => c.BolsasSilo).IsRequired();
        builder.Property(c => c.Melaza).HasColumnType("decimal(10,2)").IsRequired();

        builder.OwnsOne(c => c.CostoJornal, dinero =>
        {
            dinero.Property(d => d.Monto).HasColumnName("CostoJornalMonto").HasColumnType("decimal(18,2)").IsRequired();
            dinero.Property(d => d.Moneda).HasColumnName("CostoJornalMoneda").HasMaxLength(3).IsRequired();
        });
    }
}
