using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProveedoresCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compras",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo_compra = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    costo_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    moneda = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cantidad_cabezas = table.Column<int>(type: "integer", nullable: true),
                    precio_por_cabeza = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    peso_promedio_kg = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_insumo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cantidad_insumo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    unidad_medida = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proveedores",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contacto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_compras_fecha",
                schema: "feedlot",
                table: "compras",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "ix_compras_proveedor",
                schema: "feedlot",
                table: "compras",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_compras_tipo",
                schema: "feedlot",
                table: "compras",
                column: "tipo_compra");

            migrationBuilder.CreateIndex(
                name: "ix_proveedores_nombre",
                schema: "feedlot",
                table: "proveedores",
                column: "nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compras",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "proveedores",
                schema: "feedlot");
        }
    }
}
