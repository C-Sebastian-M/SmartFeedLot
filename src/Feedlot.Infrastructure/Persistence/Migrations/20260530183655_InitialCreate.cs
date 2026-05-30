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
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    numero_arete = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sexo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    raza = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: true),
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
                name: "categorias_gasto",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_gasto", x => x.id);
                });

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
                name: "prestamos",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    capital = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tasa_mensual = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    n_cuotas = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    capital_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prestamos", x => x.id);
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
                name: "socios",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    participacion = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socios", x => x.id);
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
                    tratamiento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tipo_evento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    proxima_dosis = table.Column<DateOnly>(type: "date", nullable: true),
                    responsable = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
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
                name: "cuotas_amortizacion",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prestamo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_cuota = table.Column<int>(type: "integer", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    cuota = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    abono_capital = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_pendiente = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    pagada = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_pago = table.Column<DateOnly>(type: "date", nullable: true),
                    abono_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP"),
                    cuota_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP"),
                    interes_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP"),
                    saldo_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuotas_amortizacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_cuotas_amortizacion_prestamos_prestamo_id",
                        column: x => x.prestamo_id,
                        principalSchema: "feedlot",
                        principalTable: "prestamos",
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
                name: "movimientos_financieros",
                schema: "feedlot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    periodo_anio = table.Column<int>(type: "integer", nullable: false),
                    periodo_mes = table.Column<int>(type: "integer", nullable: false),
                    categoria_gasto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    socio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    registrado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto_moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_financieros", x => x.id);
                    table.ForeignKey(
                        name: "FK_movimientos_financieros_categorias_gasto_categoria_gasto_id",
                        column: x => x.categoria_gasto_id,
                        principalSchema: "feedlot",
                        principalTable: "categorias_gasto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimientos_financieros_socios_socio_id",
                        column: x => x.socio_id,
                        principalSchema: "feedlot",
                        principalTable: "socios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "ux_categorias_gasto_nombre",
                schema: "feedlot",
                table: "categorias_gasto",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compradores_nombre",
                schema: "feedlot",
                table: "compradores",
                column: "nombre");

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
                name: "ix_consumos_lote_fecha",
                schema: "feedlot",
                table: "consumos_alimenticios",
                columns: new[] { "lote_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ux_cuotas_amortizacion_prestamo_cuota",
                schema: "feedlot",
                table: "cuotas_amortizacion",
                columns: new[] { "prestamo_id", "numero_cuota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_eventos_sanitarios_animal_fecha",
                schema: "feedlot",
                table: "eventos_sanitarios",
                columns: new[] { "animal_id", "fecha_evento" });

            migrationBuilder.CreateIndex(
                name: "ix_eventos_sanitarios_proxima_dosis",
                schema: "feedlot",
                table: "eventos_sanitarios",
                column: "proxima_dosis",
                filter: "\"proxima_dosis\" IS NOT NULL");

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
                name: "IX_movimientos_financieros_categoria_gasto_id",
                schema: "feedlot",
                table: "movimientos_financieros",
                column: "categoria_gasto_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimientos_financieros_origen",
                schema: "feedlot",
                table: "movimientos_financieros",
                column: "origen");

            migrationBuilder.CreateIndex(
                name: "ix_movimientos_financieros_periodo",
                schema: "feedlot",
                table: "movimientos_financieros",
                columns: new[] { "periodo_anio", "periodo_mes" });

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_financieros_socio_id",
                schema: "feedlot",
                table: "movimientos_financieros",
                column: "socio_id");

            migrationBuilder.CreateIndex(
                name: "ix_pesajes_animal_fecha",
                schema: "feedlot",
                table: "pesajes",
                columns: new[] { "animal_id", "fecha_pesaje" });

            migrationBuilder.CreateIndex(
                name: "ix_proveedores_nombre",
                schema: "feedlot",
                table: "proveedores",
                column: "nombre");

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
                name: "ux_socios_nombre",
                schema: "feedlot",
                table: "socios",
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
                name: "animal_lotes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "compradores",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "compras",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "consumos_alimenticios",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "cuotas_amortizacion",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "eventos_sanitarios",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "ingredientes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "movimientos_financieros",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "pesajes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "proveedores",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "racion_ingredientes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "venta_items",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "lotes",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "prestamos",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "categorias_gasto",
                schema: "feedlot");

            migrationBuilder.DropTable(
                name: "socios",
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

            migrationBuilder.DropTable(
                name: "ventas",
                schema: "feedlot");
        }
    }
}
