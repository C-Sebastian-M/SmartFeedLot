using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedores");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Contacto).HasColumnName("contacto").HasMaxLength(200);
        builder.Property(p => p.Telefono).HasColumnName("telefono").HasMaxLength(50);
        builder.Property(p => p.Email).HasColumnName("email").HasMaxLength(200);

        builder.HasIndex(p => p.Nombre).HasDatabaseName("ix_proveedores_nombre");
    }
}