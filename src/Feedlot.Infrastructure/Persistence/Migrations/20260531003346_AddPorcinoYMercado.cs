using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPorcinoYMercado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "canal",
                schema: "feedlot",
                table: "ventas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "comision_pct",
                schema: "feedlot",
                table: "ventas",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "costo_transporte",
                schema: "feedlot",
                table: "ventas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transporte_moneda",
                schema: "feedlot",
                table: "ventas",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "lotes_cerdos",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    n_animales = table.Column<int>(type: "integer", nullable: false),
                    peso_promedio_kg = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ciclo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    camada_id = table.Column<Guid>(type: "uuid", nullable: true),
                    precio_venta_kg = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fecha_venta = table.Column<DateOnly>(type: "date", nullable: true),
                    precio_venta_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lotes_cerdos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "marranas",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_compra = table.Column<DateOnly>(type: "date", nullable: false),
                    costo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    costo_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marranas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "precios_mercado",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    especie = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    precio_por_kg = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fuente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precios_mercado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "camadas",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    marrana_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    n_lechones = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_camadas", x => x.id);
                    table.ForeignKey(
                        name: "FK_camadas_marranas_marrana_id",
                        column: x => x.marrana_id,
                        principalSchema: "feedlot",
                        principalTable: "marranas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_camadas_marrana_id",
                schema: "feedlot",
                table: "camadas",
                column: "marrana_id");

            migrationBuilder.CreateIndex(
                name: "ux_lotes_cerdos_codigo",
                schema: "feedlot",
                table: "lotes_cerdos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_marranas_identificacion",
                schema: "feedlot",
                table: "marranas",
                column: "identificacion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "camadas",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "lotes_cerdos",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "precios_mercado",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "marranas",
                schema: "feedlot");

            migrationBuilder.DropColumn(
                name: "canal",
                schema: "feedlot",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "comision_pct",
                schema: "feedlot",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "costo_transporte",
                schema: "feedlot",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "transporte_moneda",
                schema: "feedlot",
                table: "ventas");
        }
    }
}
