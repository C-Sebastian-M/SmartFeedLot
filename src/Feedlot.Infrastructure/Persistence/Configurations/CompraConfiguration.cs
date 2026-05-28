using Feedlot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feedlot.Infrastructure.Persistence.Configurations;

public sealed class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("compras");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(c => c.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        builder.Property(c => c.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(c => c.TipoCompra).HasColumnName("tipo_compra").HasMaxLength(10).IsRequired();
        builder.Property(c => c.CostoTotal).HasColumnName("costo_total").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(c => c.Moneda).HasColumnName("moneda").HasMaxLength(5).IsRequired();
        builder.Property(c => c.Descripcion).HasColumnName("descripcion").HasMaxLength(500);

        builder.Property(c => c.CantidadCabezas).HasColumnName("cantidad_cabezas");
        builder.Property(c => c.PrecioPorCabeza).HasColumnName("precio_por_cabeza").HasColumnType("decimal(18,2)");
        builder.Property(c => c.PesoPromedioKg).HasColumnName("peso_promedio_kg").HasColumnType("decimal(10,2)");
        builder.Property(c => c.LoteId).HasColumnName("lote_id");

        builder.Property(c => c.TipoInsumo).HasColumnName("tipo_insumo").HasMaxLength(30);
        builder.Property(c => c.CantidadInsumo).HasColumnName("cantidad_insumo").HasColumnType("decimal(18,2)");
        builder.Property(c => c.UnidadMedida).HasColumnName("unidad_medida").HasMaxLength(20);

        builder.HasIndex(c => c.ProveedorId).HasDatabaseName("ix_compras_proveedor");
        builder.HasIndex(c => c.Fecha).HasDatabaseName("ix_compras_fecha");
        builder.HasIndex(c => c.TipoCompra).HasDatabaseName("ix_compras_tipo");
    }
}