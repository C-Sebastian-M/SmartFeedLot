using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class EstanciaAnimalConfiguration : IEntityTypeConfiguration<EstanciaAnimal>
{
    public void Configure(EntityTypeBuilder<EstanciaAnimal> builder)
    {
        builder.ToTable("EstanciasAnimales");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PotreroId).IsRequired();
        builder.Property(e => e.AnimalId).IsRequired();
        builder.Property(e => e.FechaEntrada).IsRequired();
        builder.Property(e => e.Salida);

        builder.HasIndex(e => new { e.AnimalId, e.PotreroId });
    }
}
