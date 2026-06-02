using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubagan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subagan_eventos",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubaganEventoId = table.Column<int>(type: "integer", nullable: false),
                    NumeroSubasta = table.Column<int>(type: "integer", nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Sede = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImportadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subagan_eventos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subagan_lotes",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubaganEventoId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    NumeroLote = table.Column<int>(type: "integer", nullable: false),
                    CodigoTipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DescripcionTipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PesoTotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PesoProm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PrecioPorKg = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Procedencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subagan_lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subagan_lotes_subagan_eventos_SubaganEventoId",
                        column: x => x.SubaganEventoId,
                        principalSchema: "feedlot",
                        principalTable: "subagan_eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subagan_eventos_SubaganEventoId",
                schema: "feedlot",
                table: "subagan_eventos",
                column: "SubaganEventoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subagan_lotes_SubaganEventoId",
                schema: "feedlot",
                table: "subagan_lotes",
                column: "SubaganEventoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subagan_lotes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "subagan_eventos",
                schema: "feedlot");
        }
    }
}
