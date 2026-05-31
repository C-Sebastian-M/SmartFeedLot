using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPresupuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "presupuestos",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo_anio = table.Column<int>(type: "integer", nullable: false),
                    periodo_mes = table.Column<int>(type: "integer", nullable: false),
                    categoria_gasto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto_presupuestado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    monto_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuestos", x => x.id);
                    table.ForeignKey(
                        name: "FK_presupuestos_categorias_gasto_categoria_gasto_id",
                        column: x => x.categoria_gasto_id,
                        principalSchema: "feedlot",
                        principalTable: "categorias_gasto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_presupuestos_categoria_gasto_id",
                schema: "feedlot",
                table: "presupuestos",
                column: "categoria_gasto_id");

            migrationBuilder.CreateIndex(
                name: "ix_presupuestos_periodo_categoria",
                schema: "feedlot",
                table: "presupuestos",
                columns: new[] { "periodo_anio", "periodo_mes", "categoria_gasto_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "presupuestos",
                schema: "feedlot");
        }
    }
}
