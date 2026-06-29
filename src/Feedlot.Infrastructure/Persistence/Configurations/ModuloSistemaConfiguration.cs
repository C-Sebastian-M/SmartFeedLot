using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class ModuloSistemaConfiguration : IEntityTypeConfiguration<ModuloSistema>
{
    public void Configure(EntityTypeBuilder<ModuloSistema> builder)
    {
        builder.ToTable("modulos_sistema");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(m => m.Clave).HasColumnName("clave").HasMaxLength(50).IsRequired();
        builder.Property(m => m.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(m => m.Activo).HasColumnName("activo").IsRequired();
        builder.Property(m => m.Orden).HasColumnName("orden").IsRequired();

        builder.HasIndex(m => m.Clave).IsUnique().HasDatabaseName("ix_modulos_sistema_clave");
    }
}
