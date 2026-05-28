using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVentasModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compradores",
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
                    table.PrimaryKey("PK_compradores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ventas",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    comprador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    moneda = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "venta_items",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    animal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    precio_venta = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    peso_venta_kg = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venta_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_venta_items_ventas_venta_id",
                        column: x => x.venta_id,
                        principalSchema: "feedlot",
                        principalTable: "ventas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_compradores_nombre",
                schema: "feedlot",
                table: "compradores",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "ix_venta_items_animal",
                schema: "feedlot",
                table: "venta_items",
                column: "animal_id");

            migrationBuilder.CreateIndex(
                name: "ix_venta_items_venta",
                schema: "feedlot",
                table: "venta_items",
                column: "venta_id");

            migrationBuilder.CreateIndex(
                name: "ix_ventas_comprador",
                schema: "feedlot",
                table: "ventas",
                column: "comprador_id");

            migrationBuilder.CreateIndex(
                name: "ix_ventas_fecha",
                schema: "feedlot",
                table: "ventas",
                column: "fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compradores",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "venta_items",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "ventas",
                schema: "feedlot");
        }
    }
}
