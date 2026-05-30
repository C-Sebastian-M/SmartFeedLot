using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class SocioConfiguration : IEntityTypeConfiguration<Socio>
{
    public void Configure(EntityTypeBuilder<Socio> builder)
    {
        builder.ToTable("socios");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(s => s.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Participacion)
            .HasColumnName("participacion")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.HasIndex(s => s.Nombre).IsUnique().HasDatabaseName("ux_socios_nombre");
    }
}
