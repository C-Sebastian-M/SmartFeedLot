using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class SubaganLoteConfiguration : IEntityTypeConfiguration<SubaganLote>
{
    public void Configure(EntityTypeBuilder<SubaganLote> builder)
    {
        builder.ToTable("subagan_lotes", "feedlot");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.SubaganEventoId).IsRequired();
        builder.Property(l => l.LoteId).IsRequired();
        builder.Property(l => l.NumeroLote).IsRequired();
        builder.Property(l => l.CodigoTipo).HasMaxLength(10).IsRequired();
        builder.Property(l => l.DescripcionTipo).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Cantidad).IsRequired();
        builder.Property(l => l.PesoTotal).HasPrecision(10, 2).IsRequired();
        builder.Property(l => l.PesoProm).HasPrecision(10, 2).IsRequired();
        builder.Property(l => l.PrecioPorKg).HasPrecision(10, 2).IsRequired();
        builder.Property(l => l.Procedencia).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Observaciones).HasMaxLength(500);
        builder.Property(l => l.Fecha).IsRequired();
    }
}
