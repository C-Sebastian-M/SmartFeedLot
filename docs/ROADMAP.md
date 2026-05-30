# Roadmap y Propuesta de Modelo de Datos — SmartFeedLot

> Proyecto: SmartFeedLot
> Fecha: 2026-05-30
> Documento acompañante de `REQUIREMENTS.md` (v2)
> **Decisión de alcance adoptada:** Plataforma de finca completa (multiespecie: bovino + porcino + agrícola)
> **Prioridad de negocio:** Control financiero primero

---

## 1. Visión

SmartFeedLot evoluciona de "gestión de feedlot bovino" a **plataforma de gestión integral de una
finca de ceba mixta**. Debe reemplazar el archivo Excel actual, que hoy concentra: seguimiento
zootécnico, costos mensuales, inversión por etapas, préstamo, proyección financiera y reparto
entre socios.

El núcleo zootécnico (animales, pesajes, lotes, sanidad, compras, ventas) ya está construido y es
sólido. El esfuerzo de aquí en adelante se concentra en el **eje financiero y de planeación**, que
es donde está el vacío y la prioridad del negocio.

---

## 2. Principios de diseño

1. **Respetar la Clean Architecture existente.** Cada módulo nuevo sigue el patrón actual:
   entidad en `Domain` → comandos/queries/handlers en `Application` → configuración EF +
   repositorio en `Infrastructure` → controller en `API` → página en `frontend`.
2. **El dinero es un Value Object.** Reutilizar `Dinero` (monto + moneda) en todo lo financiero.
   No introducir `decimal` sueltos para valores monetarios.
3. **Dimensión temporal explícita.** Lo financiero del Excel es mensual. Los nuevos agregados
   deben tener periodo (año/mes) como ciudadano de primera clase, no inferido de fechas.
4. **Multiespecie sin duplicar.** Bovino y porcino comparten conceptos (animal, lote, peso, costo,
   venta). Modelar con una base común y especialización, no copiando módulos enteros.
5. **Exportable.** Todo reporte financiero debe poder salir a Excel/PDF — es condición para que el
   negocio abandone la hoja de cálculo.

---

## 3. Decisión de modelado multiespecie

Para evitar duplicar todo el módulo de bovinos al añadir cerdos, se propone:

- Mantener `Animal` (bovino) como está.
- Introducir un enum `EspecieAnimal { Bovino, Porcino }` y un agregado paralelo `Lechon`/`Cerdo`
  **solo** donde el ciclo difiere realmente (camadas, preceba, ciclos cuatrimestrales).
- Unificar el **lado financiero** (costos, ventas, P&L) con una referencia polimórfica ligera
  (`origen`: bovino / porcino / agrícola / general), de modo que un costo o ingreso pueda
  imputarse a cualquier línea de negocio sin tablas separadas.

> Alternativa considerada y descartada: un único `Animal` genérico con `especie`. Se descartó
> porque el ciclo porcino (camada de ~48, venta por lote, no individual) no encaja con el
> seguimiento individual del bovino. Mejor dos agregados que comparten el plano financiero.

---

## 4. Entidades nuevas propuestas

Agrupadas por módulo. (Ag. = Aggregate Root)

### Finanzas (prioridad 1)

| Entidad | Tipo | Campos clave | Fuente Excel |
|---------|------|--------------|--------------|
| `CategoriaGasto` | Catálogo | nombre, tipo (Directo/Indirecto/Operativo/Inversión) | `COSTOS Y GASTOS MENSUALES` |
| `MovimientoFinanciero` | Ag. | fecha, periodo(año,mes), categoria, `Dinero`, origen (Bovino/Porcino/Agricola/General), descripción, socio? | matriz mensual |
| `Prestamo` | Ag. | capital `Dinero`, tasaMensual, nCuotas, fechaInicio, `CuotaAmortizacion[]` | `AMORTIZ CREDITO` |
| `CuotaAmortizacion` | Entidad | nCuota, fecha, cuota, interes, abonoCapital, saldoPendiente | `AMORTIZ CREDITO` |
| `Presupuesto` | Ag. | periodo, categoria, montoPresupuestado | el Excel completo es el presupuesto |

> `Prestamo` genera su tabla de amortización con la fórmula de cuota fija (sistema francés):
> `cuota = capital × i / (1 − (1+i)^−n)`. Verificada contra el Excel: $20M, i=1.79%, n=12 → ~$1.868M.

### Planeación / Inversión (prioridad 1, junto a finanzas)

| Entidad | Tipo | Campos clave | Fuente Excel |
|---------|------|--------------|--------------|
| `EtapaInversion` | Ag. | numero (1-5), nombre, `ItemInversion[]`, totalRealizado, totalPendiente | `ETAPAS DE INVERSION` |
| `ItemInversion` | Entidad | producto, costo `Dinero`, observación, estado (OK/Pendiente), porcentajeAvance, socio? | `ETAPAS DE INVERSION` |
| `Socio` | Ag. | nombre, participación | `INVERSION ESTEFA`/`LEVIR` |
| `AporteSocio` | Entidad | socio, item/movimiento, `Dinero` | columnas Estefania/Levir |

### Operación (prioridad 2)

| Entidad | Tipo | Campos clave | Fuente Excel |
|---------|------|--------------|--------------|
| `Potrero` | Ag. | nombre, capacidad, `EstanciaAnimal[]` | `SEGUIMIENTO GANADO` (potreros) |
| `EstanciaAnimal` | Entidad | animal, potrero, fechaEntrada, fechaSalida | entrada/salida por potrero |
| `Empleado` | Ag. | nombre, pagoMensual | `MANO DE OBRA` |
| `ActividadManoObra` | Entidad | empleado, tipo, fecha, `Dinero` | `MANO DE OBRA` |
| `CultivoCaña` | Ag. | `CorteCaña[]`, calles totales | `PRODUCCION CAÑA` |
| `CorteCaña` | Entidad | fecha, nCalles, horas, bolsasSilo, melaza, costo | `PRODUCCION CAÑA` |
| `LoteSilo` | Ag. | fechaProducción, bolsas, costoUnitario, origenCaña | caña → silo |

### Porcino (prioridad 3)

| Entidad | Tipo | Campos clave | Fuente Excel |
|---------|------|--------------|--------------|
| `Marrana` | Ag. | identificación, fechaCompra, costo, `Camada[]` | `PROYECCION FINANCIERA` |
| `Camada` | Entidad | fechaNacimiento, nLechones, estado (Preceba/Ceba/Vendida) | proyección porcina |
| `LoteCerdos` | Ag. | camada, nAnimales, pesoProm, ciclo | venta por lote |

### Mercado (prioridad 3)

| Entidad | Tipo | Campos clave | Fuente Excel |
|---------|------|--------------|--------------|
| `PrecioMercado` | Ag. | fecha, especie/tipo, precioPorKg, fuente | `INFO CARNICERIA` |
| extensión a `Venta` | — | canal (Directa/Subasta), comisiónPct, transporte | `SUBAGAN` |

---

## 5. Cambios a entidades existentes

| Entidad | Cambio | RF |
|---------|--------|-----|
| `CostoOperativo` | Migrar a `MovimientoFinanciero` con catálogo de categorías y periodo mensual. Deprecar enum `CategoriaCosto` de 2 valores | RF-020/021/022 |
| `Venta` | Añadir `canal`, `comisionPct`, `costoTransporte` | RF-042 |
| `Animal` | Añadir `especie` (default Bovino); relación con `Potrero` vía `EstanciaAnimal` | RF-005, multiespecie |
| `Racion`/`ConsumoAlimenticio` | Permitir que un ingrediente sea `LoteSilo` (producido) | RF-015 |
| `IndicadorProductivoService` | Exponer "incremento mensual aprox" como en el Excel | RF-007 |

> Atención migraciones: `CostoOperativo → MovimientoFinanciero` es el cambio de esquema más grande.
> Hacerlo **antes** del primer despliegue productivo con datos reales (ver nota de migraciones abajo).

---

## 6. Plan por fases (ordenado por control financiero)

### Fase 0 — Higiene previa (1-2 días)
- Quitar `logs/`, `api.log`, `api.err` del control de versiones (añadir a `.gitignore`).
- Mover `JwtSettings.SecretKey` a variables de entorno / user-secrets.
- Sembrar catálogo inicial de `CategoriaGasto` con las categorías reales del Excel.
- **Antes de datos productivos:** consolidar las 7 migraciones en una `InitialCreate` limpia.

### Fase 1 — Control financiero (núcleo de la prioridad) ⭐
Objetivo: el dueño ve costos, utilidad y deuda sin abrir Excel.
- `CategoriaGasto` + `MovimientoFinanciero` (reemplaza `CostoOperativo`).
- Vista de **costos mensuales** (matriz mes × categoría), filtrable por línea de negocio.
- `Prestamo` + tabla de amortización + alerta de cuota próxima (RF-025, RF-050).
- **Reportes financieros**: Estado de Resultados (P&L) y flujo de caja por periodo (RF-023/024).
- Exportación de reportes a Excel/PDF (RF-047).

### Fase 2 — Planeación e inversión
- `EtapaInversion` + `ItemInversion` (estado, % avance, realizado vs pendiente).
- `Socio` + `AporteSocio` y reparto de costos/inversión por socio.
- Alerta de ítem de inversión pendiente (RF-051).
- Comparación real vs presupuesto (`Presupuesto`, RF-052).

### Fase 3 — Operación
- `Potrero` + `EstanciaAnimal` (ubicación física, distinta del lote).
- `Empleado` + `ActividadManoObra`.
- `CultivoCaña` + `CorteCaña` + `LoteSilo`, enlazado a alimentación.
- Eventos sanitarios recurrentes (baño, vitamina, purgante).
- Gráficos de evolución de peso por animal.

### Fase 4 — Multiespecie (porcino) y mercado
- `Marrana` + `Camada` + `LoteCerdos` con su ciclo y P&L propio.
- `PrecioMercado` + canal de subasta con comisión en `Venta`.
- P&L consolidado finca (bovino + porcino + agrícola).

### Fase 5 — Calidad y robustez (transversal, empezar ya)
- Proyecto de **pruebas unitarias** en `Domain`/`Application` (value objects, cálculo de
  amortización, GMD/ICA, P&L). Hoy no hay tests.
- Pruebas de frontend para flujos críticos.
- Validación de la fórmula de amortización contra el Excel como test de regresión.

---

## 7. Nota sobre migraciones de base de datos

El repo acumula 7 migraciones en ~6 días, varias correctivas (`MakeRazaNullable`,
`AddNombreAndNullableFechaNacimiento`). Esto es normal en desarrollo, pero el cambio
`CostoOperativo → MovimientoFinanciero` de la Fase 1 es estructural.

**Recomendación:** mientras no haya datos productivos, consolidar todas las migraciones en una sola
`InitialCreate` limpia que ya incluya el modelo financiero nuevo. Una vez en producción con datos
reales, los cambios de esquema (nullabilidad, renombres, splits de tabla) se vuelven caros y
arriesgados — por eso conviene estabilizar el modelo financiero **antes** de ese hito.

---

## 8. Resumen de prioridades

| Fase | Foco | RF cubiertos | Dependencias |
|------|------|--------------|--------------|
| 0 | Higiene | — | ninguna |
| 1 ⭐ | Control financiero | RF-020 a 025, 045, 047, 050 | Fase 0 |
| 2 | Inversión y socios | RF-032 a 037, 051, 052 | Fase 1 (Dinero, periodos) |
| 3 | Operación | RF-005, 008, 015, 026-028, 038-039 | núcleo existente |
| 4 | Porcino y mercado | RF-029 a 031, 042, 043 | Fase 1 (plano financiero) |
| 5 | Calidad | — (transversal) | empezar en paralelo |

El camino crítico es **Fase 0 → Fase 1**: entrega el mayor valor (reemplazar el Excel financiero)
y establece las piezas (`Dinero`, periodos mensuales, categorías, exportación) de las que dependen
todas las demás fases.
