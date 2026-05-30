using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class PotreroConfiguration : IEntityTypeConfiguration<Potrero>
{
    public void Configure(EntityTypeBuilder<Potrero> builder)
    {
        builder.ToTable("Potreros");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Capacidad).IsRequired();

        builder.HasMany(p => p.Estancias)
            .WithOne()
            .HasForeignKey(e => e.PotreroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
