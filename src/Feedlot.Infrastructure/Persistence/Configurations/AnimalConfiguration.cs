using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API del Aggregate Animal.
///
/// Decisiones clave:
/// - Los Value Objects (Peso, CodigoIdentificacion, Dinero) se mapean como
///   "owned types" o columnas individuales — nunca como tablas separadas,
///   ya que son partes del aggregate, no entidades independientes.
/// - Los enums se guardan como strings para legibilidad en la BD y
///   resistencia a reordenamiento de valores.
/// - Las entidades internas (Pesaje, EventoSanitario) se configuran aquí
///   como owned collections o navigations del aggregate.
/// </summary>
public sealed class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.ToTable("animals");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // El dominio genera el ID, no la BD.

        // Value Object: CodigoIdentificacion → columna simple.
        builder.Property(a => a.CodigoIdentificacion)
            .HasColumnName("codigo_identificacion")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                co => co.Valor,
                valor => CodigoIdentificacion.Crear(valor));

        builder.HasIndex(a => a.CodigoIdentificacion)
            .IsUnique()
            .HasDatabaseName("ix_animals_codigo_identificacion");

        builder.Property(a => a.NumeroArete)
            .HasColumnName("numero_arete")
            .HasMaxLength(50)
            .IsRequired();

        // Value Object: Peso de ingreso → columna decimal.
        builder.Property(a => a.PesoIngreso)
            .HasColumnName("peso_ingreso_kg")
            .HasPrecision(10, 3)
            .IsRequired()
            .HasConversion(
                p => p.Kilogramos,
                kg => Peso.Crear(kg));

        // Value Object: Dinero (PrecioCompra) → dos columnas.
        builder.Property(a => a.PrecioCompra)
            .HasColumnName("precio_compra")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                d => d.Monto,
                monto => Dinero.Crear(monto, "COP")); // Moneda se guarda en columna separada.

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

        // Navigations internas del aggregate — EF Core las rastrea como owned collections.
        builder.HasMany(a => a.Pesajes)
            .WithOne()
            .HasForeignKey(p => p.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.EventosSanitarios)
            .WithOne()
            .HasForeignKey(e => e.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Campo de solo lectura — acceso por nombre de campo privado para la lista interna.
        builder.Navigation(a => a.Pesajes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(a => a.EventosSanitarios)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
