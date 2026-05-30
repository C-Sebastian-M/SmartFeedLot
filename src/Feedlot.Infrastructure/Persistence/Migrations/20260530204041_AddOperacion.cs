using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CultivosCania",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CallesTotales = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultivosCania", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PagoMensualMonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PagoMensualMoneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LotesSilo",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorteCaniaId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaProduccion = table.Column<DateOnly>(type: "date", nullable: false),
                    Bolsas = table.Column<int>(type: "integer", nullable: false),
                    CostoUnitarioMonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CostoUnitarioMoneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotesSilo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Potreros",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Capacidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Potreros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CortesCania",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CultivoCaniaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    NCalles = table.Column<int>(type: "integer", nullable: false),
                    Horas = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    BolsasSilo = table.Column<int>(type: "integer", nullable: false),
                    Melaza = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CostoJornalMonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CostoJornalMoneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CortesCania", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CortesCania_CultivosCania_CultivoCaniaId",
                        column: x => x.CultivoCaniaId,
                        principalSchema: "feedlot",
                        principalTable: "CultivosCania",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActividadesManoObra",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpleadoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    CostoMonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CostoMoneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadesManoObra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActividadesManoObra_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalSchema: "feedlot",
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstanciasAnimales",
                schema: "feedlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PotreroId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaEntrada = table.Column<DateOnly>(type: "date", nullable: false),
                    Salida = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstanciasAnimales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstanciasAnimales_Potreros_PotreroId",
                        column: x => x.PotreroId,
                        principalSchema: "feedlot",
                        principalTable: "Potreros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesManoObra_EmpleadoId",
                schema: "feedlot",
                table: "ActividadesManoObra",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_CortesCania_CultivoCaniaId",
                schema: "feedlot",
                table: "CortesCania",
                column: "CultivoCaniaId");

            migrationBuilder.CreateIndex(
                name: "IX_EstanciasAnimales_AnimalId_PotreroId",
                schema: "feedlot",
                table: "EstanciasAnimales",
                columns: new[] { "AnimalId", "PotreroId" });

            migrationBuilder.CreateIndex(
                name: "IX_EstanciasAnimales_PotreroId",
                schema: "feedlot",
                table: "EstanciasAnimales",
                column: "PotreroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActividadesManoObra",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "CortesCania",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "EstanciasAnimales",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "LotesSilo",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "Empleados",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "CultivosCania",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "Potreros",
                schema: "feedlot");
        }
    }
}
