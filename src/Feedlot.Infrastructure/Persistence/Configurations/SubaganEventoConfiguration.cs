using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class SubaganEventoConfiguration : IEntityTypeConfiguration<SubaganEvento>
{
    public void Configure(EntityTypeBuilder<SubaganEvento> builder)
    {
        builder.ToTable("subagan_eventos", "feedlot");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SubaganEventoId).IsRequired();
        builder.HasIndex(e => e.SubaganEventoId).IsUnique();

        builder.Property(e => e.NumeroSubasta);
        builder.Property(e => e.Fecha).IsRequired();
        builder.Property(e => e.Sede).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ImportadoEn).IsRequired();

        builder.HasMany(e => e.Lotes)
            .WithOne()
            .HasForeignKey(l => l.SubaganEventoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
