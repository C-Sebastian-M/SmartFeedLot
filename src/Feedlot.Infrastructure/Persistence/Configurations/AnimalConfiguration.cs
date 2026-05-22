using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.ToTable("animals");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Value Object: CodigoIdentificacion → columna simple con conversión.
        builder.Property(a => a.CodigoIdentificacion)
            .HasColumnName("codigo_identificacion")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                co => co.Valor,
                valor => CodigoIdentificacion.Crear(valor));

        // CORRECCIÓN: el índice se define por nombre de columna, no por propiedad,
        // cuando la propiedad tiene HasConversion. Así EF Core puede construirlo
        // correctamente en la migración.
        builder.HasIndex("codigo_identificacion")
            .IsUnique()
            .HasDatabaseName("ix_animals_codigo_identificacion");

        builder.Property(a => a.NumeroArete)
            .HasColumnName("numero_arete")
            .HasMaxLength(50)
            .IsRequired();

        // Value Object: Peso de ingreso
        builder.Property(a => a.PesoIngreso)
            .HasColumnName("peso_ingreso_kg")
            .HasPrecision(10, 3)
            .IsRequired()
            .HasConversion(
                p => p.Kilogramos,
                kg => Peso.Crear(kg));

        // Value Object: Dinero (PrecioCompra) → monto en columna principal.
        // La moneda se guarda como shadow property separada.
        builder.Property(a => a.PrecioCompra)
            .HasColumnName("precio_compra")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                monto => Dinero.Crear(monto, "COP"));

        builder.Property<string>("precio_compra_moneda")
            .HasColumnName("precio_compra_moneda")
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("COP");

        builder.Property(a => a.Sexo)
            .HasColumnName("sexo")
            .HasMaxLength(10)
            .IsRequired()
            .HasConversion(
                s => s.ToString(),
                s => Enum.Parse<Sexo>(s));

        builder.Property(a => a.Raza)
            .HasColumnName("raza")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.FechaNacimiento)
            .HasColumnName("fecha_nacimiento")
            .IsRequired();

        builder.Property(a => a.FechaIngreso)
            .HasColumnName("fecha_ingreso")
            .IsRequired();

        builder.Property(a => a.EstadoProductivo)
            .HasColumnName("estado_productivo")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                e => e.ToString(),
                e => Enum.Parse<EstadoProductivo>(e));

        builder.Property(a => a.EstadoSanitario)
            .HasColumnName("estado_sanitario")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                e => e.ToString(),
                e => Enum.Parse<EstadoSanitario>(e));

        builder.HasMany(a => a.Pesajes)
            .WithOne()
            .HasForeignKey(p => p.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.EventosSanitarios)
            .WithOne()
            .HasForeignKey(e => e.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Pesajes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(a => a.EventosSanitarios)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
