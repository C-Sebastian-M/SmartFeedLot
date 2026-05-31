using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class CamadaConfiguration : IEntityTypeConfiguration<Camada>
{
    public void Configure(EntityTypeBuilder<Camada> builder)
    {
        builder.ToTable("camadas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(c => c.MarranaId)
            .HasColumnName("marrana_id")
            .IsRequired();

        builder.Property(c => c.FechaNacimiento)
            .HasColumnName("fecha_nacimiento")
            .IsRequired();

        builder.Property(c => c.NLechones)
            .HasColumnName("n_lechones")
            .IsRequired();

        builder.Property(c => c.Estado)
            .HasColumnName("estado")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion<string>();
    }
}
