using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class EtapaInversionConfiguration : IEntityTypeConfiguration<EtapaInversion>
{
    public void Configure(EntityTypeBuilder<EtapaInversion> builder)
    {
        builder.ToTable("EtapasInversion");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Numero).IsRequired();
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(200);

        builder.HasMany(e => e.Items)
            .WithOne()
            .HasForeignKey(i => i.EtapaInversionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
