using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class CultivoCaniaConfiguration : IEntityTypeConfiguration<CultivoCania>
{
    public void Configure(EntityTypeBuilder<CultivoCania> builder)
    {
        builder.ToTable("CultivosCania");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CallesTotales).IsRequired();

        builder.HasMany(c => c.Cortes)
            .WithOne()
            .HasForeignKey(cc => cc.CultivoCaniaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
