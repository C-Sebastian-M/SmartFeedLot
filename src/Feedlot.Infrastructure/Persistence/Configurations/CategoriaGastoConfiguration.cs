using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class CategoriaGastoConfiguration : IEntityTypeConfiguration<CategoriaGasto>
{
    public void Configure(EntityTypeBuilder<CategoriaGasto> builder)
    {
        builder.ToTable("categorias_gasto");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(c => c.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(c => c.Nombre).IsUnique().HasDatabaseName("ux_categorias_gasto_nombre");
    }
}
