using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feedlot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "feedlot");

            migrationBuilder.CreateTable(
                name: "animals",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numero_arete = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sexo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    raza = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    peso_ingreso_kg = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    precio_compra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: false),
                    estado_productivo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado_sanitario = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    precio_compra_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_animals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consumos_alimenticios",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    racion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cantidad_kg = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    costo_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    registrado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    costo_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consumos_alimenticios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingredientes",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    costo_kg = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    proteina_pct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    unidad_medida = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    costo_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingredientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lotes",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    capacidad_maxima = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lotes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "raciones",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    costo_kg = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    proteina_pct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    energia_mcal = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    costo_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raciones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    nombre_completo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ultimo_acceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eventos_sanitarios",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    animal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_evento = table.Column<DateOnly>(type: "date", nullable: false),
                    diagnostico = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    severidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tratamiento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_sanitarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_eventos_sanitarios_animals_animal_id",
                        column: x => x.animal_id,
                        principalSchema: "feedlot",
                        principalTable: "animals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pesajes",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    animal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_pesaje = table.Column<DateOnly>(type: "date", nullable: false),
                    peso_kg = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pesajes", x => x.id);
                    table.ForeignKey(
                        name: "FK_pesajes_animals_animal_id",
                        column: x => x.animal_id,
                        principalSchema: "feedlot",
                        principalTable: "animals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "animal_lotes",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    animal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_egreso = table.Column<DateOnly>(type: "date", nullable: true),
                    motivo_ingreso = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    motivo_egreso = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    es_activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_animal_lotes", x => x.id);
                    table.ForeignKey(
                        name: "FK_animal_lotes_lotes_lote_id",
                        column: x => x.lote_id,
                        principalSchema: "feedlot",
                        principalTable: "lotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "racion_ingredientes",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    racion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingrediente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proporcion_pct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_racion_ingredientes", x => x.id);
                    table.ForeignKey(
                        name: "FK_racion_ingredientes_raciones_racion_id",
                        column: x => x.racion_id,
                        principalSchema: "feedlot",
                        principalTable: "raciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "feedlot",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "feedlot",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "feedlot",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_animal_lotes_animal_activo",
                schema: "feedlot",
                table: "animal_lotes",
                columns: new[] { "animal_id", "es_activo" });

            migrationBuilder.CreateIndex(
                name: "ix_animal_lotes_lote_activo",
                schema: "feedlot",
                table: "animal_lotes",
                columns: new[] { "lote_id", "es_activo" });

            migrationBuilder.CreateIndex(
                name: "ix_animals_codigo_identificacion",
                schema: "feedlot",
                table: "animals",
                column: "codigo_identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_consumos_lote_fecha",
                schema: "feedlot",
                table: "consumos_alimenticios",
                columns: new[] { "lote_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_eventos_sanitarios_animal_fecha",
                schema: "feedlot",
                table: "eventos_sanitarios",
                columns: new[] { "animal_id", "fecha_evento" });

            migrationBuilder.CreateIndex(
                name: "ix_eventos_sanitarios_severidad",
                schema: "feedlot",
                table: "eventos_sanitarios",
                column: "severidad");

            migrationBuilder.CreateIndex(
                name: "ix_ingredientes_nombre",
                schema: "feedlot",
                table: "ingredientes",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lotes_codigo",
                schema: "feedlot",
                table: "lotes",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lotes_estado",
                schema: "feedlot",
                table: "lotes",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_pesajes_animal_fecha",
                schema: "feedlot",
                table: "pesajes",
                columns: new[] { "animal_id", "fecha_pesaje" });

            migrationBuilder.CreateIndex(
                name: "ix_racion_ingredientes_unique",
                schema: "feedlot",
                table: "racion_ingredientes",
                columns: new[] { "racion_id", "ingrediente_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_raciones_nombre",
                schema: "feedlot",
                table: "raciones",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_nombre",
                schema: "feedlot",
                table: "roles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                schema: "feedlot",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "feedlot",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "animal_lotes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "consumos_alimenticios",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "eventos_sanitarios",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "ingredientes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "pesajes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "racion_ingredientes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "lotes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "animals",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "raciones",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "users",
                schema: "feedlot");
        }
    }
}
