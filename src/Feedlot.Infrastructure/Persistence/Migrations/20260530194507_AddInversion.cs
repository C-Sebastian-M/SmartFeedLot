using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AportesSocios",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SocioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemInversionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AportesSocios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EtapasInversion",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtapasInversion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsInversion",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EtapaInversionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Producto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CostoMonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CostoMoneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PorcentajeAvance = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsInversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsInversion_EtapasInversion_EtapaInversionId",
                        column: x => x.EtapaInversionId,
                        principalSchema: "feedlot",
                        principalTable: "EtapasInversion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AportesSocios_SocioId_ItemInversionId",
                schema: "feedlot",
                table: "AportesSocios",
                columns: new[] { "SocioId", "ItemInversionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemsInversion_EtapaInversionId",
                schema: "feedlot",
                table: "ItemsInversion",
                column: "EtapaInversionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AportesSocios",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "ItemsInversion",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "EtapasInversion",
                schema: "feedlot");
        }
    }
}
