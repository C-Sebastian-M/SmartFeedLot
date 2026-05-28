using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class CompradorConfiguration : IEntityTypeConfiguration<Comprador>
{
    public void Configure(EntityTypeBuilder<Comprador> builder)
    {
        builder.ToTable("compradores");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Contacto).HasColumnName("contacto").HasMaxLength(200);
        builder.Property(c => c.Telefono).HasColumnName("telefono").HasMaxLength(50);
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(200);

        builder.HasIndex(c => c.Nombre).HasDatabaseName("ix_compradores_nombre");
    }
}
