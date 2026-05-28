using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVacunasFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "proxima_dosis",
                schema: "feedlot",
                table: "eventos_sanitarios",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "responsable",
                schema: "feedlot",
                table: "eventos_sanitarios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_evento",
                schema: "feedlot",
                table: "eventos_sanitarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_eventos_sanitarios_proxima_dosis",
                schema: "feedlot",
                table: "eventos_sanitarios",
                column: "proxima_dosis",
                filter: "\"proxima_dosis\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_eventos_sanitarios_proxima_dosis",
                schema: "feedlot",
                table: "eventos_sanitarios");

            migrationBuilder.DropColumn(
                name: "proxima_dosis",
                schema: "feedlot",
                table: "eventos_sanitarios");

            migrationBuilder.DropColumn(
                name: "responsable",
                schema: "feedlot",
                table: "eventos_sanitarios");

            migrationBuilder.DropColumn(
                name: "tipo_evento",
                schema: "feedlot",
                table: "eventos_sanitarios");
        }
    }
}
