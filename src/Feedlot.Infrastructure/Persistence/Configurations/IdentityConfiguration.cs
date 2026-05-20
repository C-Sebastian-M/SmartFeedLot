using Feedlot.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasColumnName("email").HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ix_users_email");

        builder.Property(u => u.NombreCompleto)
            .HasColumnName("nombre_completo").HasMaxLength(200).IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash").IsRequired();

        builder.Property(u => u.Activo).HasColumnName("activo").IsRequired();

        builder.Property(u => u.CreadoEn)
            .HasColumnName("creado_en").IsRequired();

        builder.Property(u => u.UltimoAcceso)
            .HasColumnName("ultimo_acceso");

        builder.HasMany(u => u.Roles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(r => r.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
        builder.HasIndex(r => r.Nombre).IsUnique().HasDatabaseName("ix_roles_nombre");
        builder.Property(r => r.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
    }
}

public sealed class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        builder.Property(ur => ur.UserId).HasColumnName("user_id");
        builder.Property(ur => ur.RoleId).HasColumnName("role_id");

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(ur => ur.RoleId);
    }
}
