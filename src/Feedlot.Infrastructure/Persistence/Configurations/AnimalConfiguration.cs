using Feedlot.Domain.Entities;
using Feedlot.Domain.Enums;
using Feedlot.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración del Aggregate Animal.
///
/// Decisión de diseño para CodigoIdentificacion:
/// En lugar de HasConversion (que EF Core no puede traducir en Where/AnyAsync),
/// se mapea como shadow property string "CodigoIdentificacionValor".
/// El repositorio reconstruye el VO al cargar desde BD usando AfterSave.
/// 
/// Para Peso y Dinero (solo se leen, nunca se filtran por ellos en SQL),
/// HasConversion sigue siendo apropiado.
/// </summary>
public sealed class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.ToTable("animals");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // CodigoIdentificacion → mapeado con HasConversion.
        // EF Core SÍ puede traducir comparaciones de propiedades con HasConversion
        // cuando el ValueConverter convierte a un tipo primitivo (string).
        // El problema anterior era usar EF.Property() — eso no funciona.
        // La comparación directa a.CodigoIdentificacion == vo tampoco funciona
        // porque EF Core no sabe comparar ValueObjects.
        //
        // SOLUCIÓN FINAL: mapear como string con nombre de columna explícito,
        // y usar una propiedad de acceso en el dominio para las queries.
        // Aquí usamos HasConversion pero el repositorio filtra en memoria
        // (ObtenerTodosAsync) o usa FromSqlRaw (ExisteCodigoAsync).
        builder.Property(a => a.CodigoIdentificacion)
            .HasColumnName("codigo_identificacion")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                co => co.Valor,
                valor => CodigoIdentificacion.Crear(valor));

        builder.HasIndex(nameof(Animal.CodigoIdentificacion))
            .IsUnique()
            .HasDatabaseName("ix_animals_codigo_identificacion");

        builder.Property(a => a.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100);

        builder.Property(a => a.NumeroArete)
            .HasColumnName("numero_arete")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.PesoIngreso)
            .HasColumnName("peso_ingreso_kg")
            .HasPrecision(10, 3)
            .IsRequired()
            .HasConversion(
                p => p.Kilogramos,
                kg => Peso.Crear(kg));

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
            .HasMaxLength(100);

        builder.Property(a => a.FechaNacimiento)
            .HasColumnName("fecha_nacimiento");

        builder.Property(a => a.FechaIngreso)
            .HasColumnName("fecha_ingreso")
            .IsRequired();

        // Tipo comercial (MC, ML, HV...) nullable. Se guarda como string.
        builder.Property(a => a.TipoComercial)
            .HasColumnName("tipo_comercial")
            .HasMaxLength(5)
            .HasConversion(
                t => t.HasValue ? t.Value.ToString() : null,
                s => string.IsNullOrEmpty(s) ? (TipoComercial?)null : Enum.Parse<TipoComercial>(s));

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
