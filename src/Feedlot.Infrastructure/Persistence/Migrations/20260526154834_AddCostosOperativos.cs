using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCostosOperativos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "costos_operativos",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    registrado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_costos_operativos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_costos_operativos_lote_fecha",
                schema: "feedlot",
                table: "costos_operativos",
                columns: new[] { "lote_id", "fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "costos_operativos",
                schema: "feedlot");
        }
    }
}
