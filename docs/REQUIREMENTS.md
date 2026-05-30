# Análisis de Requerimientos Funcionales — SmartFeedLot (v2)

> Proyecto: SmartFeedLot
> Versión: 2.0
> Fecha: 2026-05-30
> Fuente de verdad: **PRESUPUESTO CEBA DE GANADO.xlsx** (operación real) + código existente

---

## Sobre esta versión

La v1 de este documento (2026-05-28) se redactó observando el software ya construido. Esta v2
invierte la dirección: toma como fuente de verdad el archivo Excel con el que el negocio opera
realmente, deriva los requisitos de cada hoja, y los contrasta contra lo que el código cubre hoy.

El cambio de enfoque revela que el alcance real es más amplio que "gestión de feedlot bovino".
El negocio es una **finca de ceba intensiva mixta**, gestionada por dos socios, con:

- Ceba de **bovinos** (núcleo, bien cubierto por el software).
- Cría de **cerdos / marranas** (presente en toda la proyección financiera; ausente en el software).
- **Producción de caña** transformada en silo como insumo de alimentación.
- Un **plan de inversión por etapas** (5 etapas: potreros → bodega/picadora → riego → ganado → corral).
- **Financiamiento con crédito** (préstamo de $20M con tabla de amortización).
- Venta por **subasta** (canal SUBAGAN, comisión 3%).

---

## Convenciones

| Símbolo | Significado |
|---------|-------------|
| ✅ | Implementado completamente |
| ⚠️ | Implementado parcialmente |
| ❌ | No implementado |
| 🆕 | Requisito nuevo, surgido del Excel, no contemplado en v1 |

---

## Mapa hoja de Excel → módulo

| Hoja Excel | Contenido | Módulo destino | Estado |
|------------|-----------|----------------|--------|
| `SEGUIMIENTO GANADO` | Animales, pesajes sucesivos, aumento, potreros | Animales + Pesajes + Potreros | ⚠️ |
| `BOBINOS` | Costeo por ternero, dieta, utilidad proyectada | Animales + Costos + Analítica | ⚠️ |
| `MANO DE OBRA` | Actividades por persona, alquiler por cabeza | Mano de obra | ❌ |
| `COSTOS Y GASTOS MENSUALES` | Matriz mes×categoría 2024-2026 | Costos mensuales | ⚠️ |
| `ETAPAS DE INVERSION` | 5 etapas, ítems, estado OK/Pendiente, % avance, reparto socios | Etapas de inversión + Socios | ❌ |
| `PRODUCCION CAÑA` | Cortes, horas, insumos silo | Producción agrícola | ❌ |
| `PROYECCION FINANCIERA` | P&L por trimestre, flujo, utilidad operacional | Reportes financieros | ❌ |
| `AMORTIZ CREDITO` | Préstamo $20M, tasa 1.79%/mes, 12 cuotas | Préstamos | ❌ |
| `SUBAGAN` / `SIGLAS SUBASTA` | Subasta, comisión 3%, transporte | Ventas (canal subasta) | ⚠️ |
| `INVERSION ESTEFA` / `INVERSION LEVIR` | Aporte por socio | Socios | ❌ |
| `COTIZACION CASA Y BODEGA`, `SISTEMA DE RIEGO`, `CORRAL` | Cotizaciones de infraestructura | Etapas de inversión (ítems) | ❌ |
| `INFO CARNICERIA` | Datos de canal cárnico / mercado | Precios de mercado | ❌ |

---

## 1. Gestión de Bovinos

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-001 | Registrar bovinos | ✅ | Coincide con `SEGUIMIENTO GANADO`: nombre, fecha nacimiento, fecha compra, peso/costo inicial |
| RF-002 | Consultar historial completo | ✅ | Pesajes, eventos sanitarios, indicadores |
| RF-003 | Actualizar información | ✅ | Modal de edición |
| RF-004 | Retirar o vender | ✅ | Estados `Vendido`/`Muerto`/`Retirado` |
| RF-005 | 🆕 Registrar **ubicación por potrero** con fecha entrada/salida | ❌ | El Excel rastrea movimiento entre potreros (bajo, limón, bodega, totumo) — concepto distinto del "lote" de engorde |

**Brechas:**
- El "potrero" (ubicación física de pastoreo) no está modelado; el software solo tiene "lote" (grupo de engorde).
- Fecha y motivo de retiro/muerte no se persisten como datos estructurados.

---

## 2. Seguimiento de Peso

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-006 | Registrar pesajes | ✅ | CRUD completo |
| RF-007 | Calcular aumento de peso | ✅ | GMD, ICA, peso ganado en `IndicadorProductivoService`. El Excel calcula "incremento mensual aprox" (~5.8 kg/mes en casos reales) |
| RF-008 | Mostrar historial/gráficos | ⚠️ | Existe `GmdChart.tsx`; falta confirmar gráfico de evolución de peso por animal |
| RF-009 | 🆕 Captura de peso desde báscula IP68 | ❌ | El plan de inversión incluye "Báscula con conector IP68 — seguimiento peso" ($2.2M, pendiente). Implica integración de captura semiautomática |

**Brechas:**
- Gráficos de evolución de peso (líneas de tiempo).
- Captura de peso vía báscula (integración hardware futura).

---

## 3. Gestión de Compras y Proveedores

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-010 | Registrar compra de ganado | ✅ | Implementado tras v1 (módulo Compras) |
| RF-011 | Registrar compra de insumos | ✅ | Módulo Compras |
| RF-012 | Gestionar proveedores | ✅ | Entidad `Proveedor`, controller y UI |

**Brechas:** ninguna mayor. (Esta sección pasó de 0% en v1 a cubierta.)

---

## 4. Alimentación y Nutrición

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-013 | Registrar alimentación | ✅ | Raciones y consumo por lote |
| RF-014 | Controlar consumo mensual | ⚠️ | El Excel agrega consumo por mes (sal, melaza, salvado, silo). Falta vista de agregación mensual |
| RF-015 | 🆕 Modelar **silo** como insumo producido internamente | ❌ | El silo no se compra: se produce a partir de la caña (ver sección 7). Debe enlazarse producción agrícola → ración |
| RF-016 | 🆕 Dieta tipo (sal, azufre, salvado, melaza, silo) como plantilla | ⚠️ | `Racion` existe; falta catálogo de ingredientes alineado a la dieta real del Excel |

**Brechas:**
- Reportes periódicos de consumo por lote/animal.
- Vínculo entre caña producida y silo consumido.

---

## 5. Gestión Sanitaria

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-017 | Registrar vacunas (próxima dosis) | ✅ | Implementado tras v1 (campo próxima dosis + alerta) |
| RF-018 | Registrar tratamientos médicos | ✅ | Diagnóstico, tratamiento, severidad |
| RF-019 | 🆕 Registrar baños y desparasitación como eventos recurrentes | ⚠️ | El Excel los trata como rutina periódica con costo; hoy caben en evento sanitario pero sin recurrencia |

**Brechas:**
- Eventos sanitarios recurrentes/programados (baño, vitamina, purgante).

---

## 6. Gestión Financiera

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-020 | Registrar costos y gastos | ⚠️ | `CostoOperativo` con solo `ManoDeObra`/`CIF`. El Excel usa muchas más categorías |
| RF-021 | Clasificar gastos | ❌ | Faltan categorías reales: Alimentación, Sanidad, Alquiler, Mano de obra, Insumos silo, Riego, Infraestructura, Agrícola |
| RF-022 | 🆕 **Costos mensuales** (matriz mes × categoría) | ❌ | `COSTOS Y GASTOS MENSUALES` es una matriz temporal 2024-2026. Requiere dimensión mes explícita |
| RF-023 | Proyección financiera (P&L) | ❌ | `PROYECCION FINANCIERA` tiene estado de resultados por trimestre: ingresos, costo de producción, utilidad bruta, gastos, utilidad operacional. Hoy solo hay `rentabilidadProyectada` por animal |
| RF-024 | 🆕 Flujo de caja proyectado | ❌ | Proyecciones de ingresos/egresos por periodo con utilidad mensual promedio |
| RF-025 | 🆕 Registrar **préstamos con amortización** | ❌ | `AMORTIZ CREDITO`: capital $20M, tasa 1.79% mensual, cuota fija ~$1.868M, 12 cuotas, tabla interés/abono/saldo |

**Brechas:**
- Sistema de categorías de gasto insuficiente (2 vs ~9 reales).
- No hay P&L, flujo de caja ni utilidad operacional agregada.
- No hay módulo de préstamos.

---

## 7. Producción Agrícola (Caña → Silo)

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-026 | 🆕 Registrar cortes de caña | ❌ | `PRODUCCION CAÑA`: fecha, n° calles cortadas, horas de trabajo, bolsas, melaza, costo jornal |
| RF-027 | 🆕 Convertir caña en silo (proceso de fermentación) | ❌ | Silo = caña + melaza + vitaminas, fermentación 1-2 meses. Salida: bolsas de silo como insumo |
| RF-028 | 🆕 Costeo de producción agrícola | ❌ | Costo por corte (horas × valor hora + insumos) vs proyección |

**Brechas:** módulo agrícola completamente ausente. Es insumo directo de la alimentación (sección 4).

---

## 8. Gestión de Cerdos / Porcinos

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-029 | 🆕 Registrar marranas de cría y lechones | ❌ | `PROYECCION FINANCIERA`: compra de 4 marranas ($700k c/u), camadas de 48 cerdos |
| RF-030 | 🆕 Ciclo de preceba/ceba porcina | ❌ | Venta de cerdos de preceba a $120k-$240k; ciclos cuatrimestrales |
| RF-031 | 🆕 Costeo y P&L porcino | ❌ | Alimentación y vacunas porcinas, infraestructura (marranera, galpón) |

**Brechas:** línea de negocio porcina completamente ausente. Aparece en toda la proyección financiera 2026-2027.

---

## 9. Etapas de Inversión

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-032 | 🆕 Registrar plan de inversión por etapas | ❌ | 5 etapas: 1) Adecuación potreros + siembra caña + registros, 2) Bodega/picadora/herramientas, 3) Sistema de riego, 4) Compra de bovinos, 5) Corral/embarcadero |
| RF-033 | 🆕 Ítems de etapa con estado y % de avance | ❌ | Cada ítem: producto, costo, observación, estado (OK/Pendiente), % avance |
| RF-034 | 🆕 Inversión realizada vs pendiente | ❌ | Totales por etapa: realizado ~$22M, pendiente ~$9.8M |

**Brechas:** módulo de proyecto/etapas/presupuesto ausente.

---

## 10. Socios y Aportes

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-035 | 🆕 Registrar socios | ❌ | Operación de dos socios: Estefania y Levir |
| RF-036 | 🆕 Repartir inversión/costos por socio | ❌ | Cada ítem de inversión tiene columna "Estefania / Costo total" y "Levir / Costo total" |
| RF-037 | 🆕 Reporte de aporte por socio | ❌ | Total aportado por cada socio (`INVERSION ESTEFA` ~$22M, `INVERSION LEVIR` ~$4.2M) |

**Brechas:** concepto de socio y reparto de costos ausente.

---

## 11. Mano de Obra

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-038 | 🆕 Registrar actividades de mano de obra | ❌ | `MANO DE OBRA`: alimentación, fumigación, mantenimiento alambre, baño/vacunación, riego, preparación de silo |
| RF-039 | 🆕 Pago mensual y alquiler por cabeza | ❌ | Pago mensual fijo ($200k) + alquiler $35k/vaca; escalado por número de animales |

**Brechas:** modelo de mano de obra/actividades ausente. (Hoy solo existe `CostoOperativo` categoría `ManoDeObra`, sin actividades ni personas.)

---

## 12. Ventas y Subasta

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-040 | Registrar ventas | ✅ | Módulo Ventas implementado tras v1 |
| RF-041 | Gestionar compradores | ✅ | Entidad `Comprador`, controller y UI |
| RF-042 | 🆕 Canal **subasta** con comisión | ⚠️ | `SUBAGAN`: venta por subasta, comisión 3%, transporte a subasta. Falta modelar canal + comisión en la venta |
| RF-043 | 🆕 Precios de mercado/carnicería | ❌ | `INFO CARNICERIA` con referencias de precio por kg/tipo |

**Brechas:**
- Canal de venta (directa vs subasta) y comisión asociada.
- Histórico de precios de mercado.

---

## 13. Reportes y Estadísticas

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-044 | Reportes de crecimiento | ✅ | Analítica por animal y por lote |
| RF-045 | Reportes financieros (P&L, flujo) | ❌ | Ver sección 6 |
| RF-046 | Reportes sanitarios | ⚠️ | Existe alerta de vacunas; falta historial exportable |
| RF-047 | 🆕 Exportar a PDF/Excel | ❌ | El negocio vive en Excel; la migración exige exportación equivalente |

---

## 14. Alertas y Notificaciones

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-048 | Vacunas pendientes | ✅ | Implementado tras v1 |
| RF-049 | Pérdida / bajo aumento de peso | ✅ | Animales ineficientes por GMD baja. El Excel muestra casos reales de aumento negativo (Vaca roja −16 kg) |
| RF-050 | 🆕 Cuota de préstamo próxima a vencer | ❌ | Derivado de la tabla de amortización |
| RF-051 | 🆕 Ítem de inversión pendiente | ❌ | Derivado del estado de etapas |
| RF-052 | Gastos elevados vs presupuesto | ❌ | El Excel ES un presupuesto; comparar real vs presupuestado |

---

## Resumen General (v2, contra el Excel)

| Área | Total RF | ✅ | ⚠️ | ❌ | Cobertura |
|------|----------|----|-----|-----|-----------|
| 1. Bovinos | 5 | 4 | 0 | 1 | 80 % |
| 2. Peso | 4 | 2 | 1 | 1 | 63 % |
| 3. Compras | 3 | 3 | 0 | 0 | 100 % |
| 4. Alimentación | 4 | 1 | 2 | 1 | 50 % |
| 5. Sanidad | 3 | 2 | 1 | 0 | 83 % |
| 6. Finanzas | 6 | 0 | 1 | 5 | 8 % |
| 7. Agrícola (caña/silo) | 3 | 0 | 0 | 3 | 0 % |
| 8. Cerdos | 3 | 0 | 0 | 3 | 0 % |
| 9. Etapas inversión | 3 | 0 | 0 | 3 | 0 % |
| 10. Socios | 3 | 0 | 0 | 3 | 0 % |
| 11. Mano de obra | 2 | 0 | 0 | 2 | 0 % |
| 12. Ventas/Subasta | 4 | 2 | 1 | 1 | 63 % |
| 13. Reportes | 4 | 1 | 1 | 2 | 38 % |
| 14. Alertas | 5 | 2 | 0 | 3 | 40 % |
| **Total** | **52** | **17** | **8** | **27** | **~40 %** |

> Nota: la cobertura del **núcleo zootécnico** (bovinos, peso, compras, sanidad, ventas) es alta
> (~75 %). La cobertura cae en todo el **eje financiero y de planeación** (finanzas, agrícola,
> cerdos, etapas, socios, mano de obra), que es justamente el peso del Excel.

---

## Brechas Prioritarias

1. **Modelo de costos real** — reemplazar las 2 categorías por el catálogo real y añadir dimensión mensual (RF-020 a RF-022). *Habilita todo lo financiero.*
2. **Préstamos con amortización** — entidad `Prestamo` + tabla de cuotas + alerta (RF-025, RF-050).
3. **Estado de resultados / P&L / flujo de caja** por periodo (RF-023, RF-024, RF-045).
4. **Etapas de inversión** con estado y avance (RF-032 a RF-034).
5. **Socios y reparto de costos** (RF-035 a RF-037).
6. **Producción agrícola (caña → silo)** enlazada a alimentación (RF-026 a RF-028).
7. **Línea porcina** (RF-029 a RF-031).
8. **Mano de obra / actividades** (RF-038, RF-039).
9. **Canal de subasta + precios de mercado** (RF-042, RF-043).
10. **Potreros** como ubicación física (RF-005).
11. **Exportación a PDF/Excel** (RF-047) — condición práctica para que el negocio abandone el Excel.

---

## Decisión de alcance pendiente (para el roadmap)

El Excel describe un negocio **mixto (bovino + porcino + agrícola)**. Hay que decidir si SmartFeedLot:

- **(A)** se mantiene como sistema **bovino** y trata cerdos/caña como costos/ingresos externos, o
- **(B)** se amplía a una **plataforma de finca** multiespecie con módulos paralelos.

Esta decisión condiciona el modelo de datos y se aborda en el documento de roadmap acompañante.
