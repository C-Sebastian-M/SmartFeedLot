# Análisis de Requerimientos Funcionales — Sistema de Gestión Ganadera

> Proyecto: SmartFeedLot
> Fecha: 2026-05-28

---

## Convenciones

| Símbolo | Significado |
|---------|-------------|
| ✅ | Implementado completamente |
| ⚠️ | Implementado parcialmente |
| ❌ | No implementado |

---

## 1. Gestión de Bovinos

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-001 | Registrar bovinos | ✅ | `nombre`, `fechaNacimiento`, `pesoIngresoKg`, `precioCompra`, `sexo`, `raza`, `fechaIngreso` |
| RF-002 | Consultar historial completo | ✅ | Detalle con pesajes, eventos sanitarios, indicadores productivos |
| RF-003 | Actualizar información | ✅ | Modal de edición con todos los campos |
| RF-004 | Retirar o vender | ✅ | Estados `Vendido`/`Muerto`/`Retirado`; movimiento entre lotes |

**Brechas:**
- No existe un campo `tipoDeGanado` explícito (el tipo se infiere de `raza`)
- Fecha y motivo de retiro/muerte no se persisten como datos estructurados (solo cambia el estado)

---

## 2. Seguimiento de Peso

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-005 | Registrar pesajes | ✅ | CRUD completo con endpoints POST, DELETE |
| RF-006 | Calcular aumento de peso | ✅ | GMD, ICA, peso ganado calculados en `IndicadorProductivoService` |
| RF-007 | Mostrar historial/gráficos | ⚠️ | Tabla de evolución de pesajes implementada; **gráficos no** |

**Brechas:**
- No hay visualización gráfica (charts/líneas de tiempo) de la evolución del peso

---

## 3. Gestión de Compras

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-008 | Registrar compra de ganado | ❌ | No existe entidad `Proveedor` ni `CompraGanado` |
| RF-009 | Registrar compra de insumos | ❌ | Solo existe `CostoOperativo` genérico |
| RF-010 | Gestionar proveedores | ❌ | No existe |

**Brechas:**
- Modelo completo de compras ausente (proveedores, órdenes de compra, insumos)

---

## 4. Alimentación y Nutrición

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-011 | Registrar alimentación | ✅ | Módulo de raciones y consumo por lote |
| RF-012 | Controlar consumo mensual | ❌ | No hay agregación mensual ni reportes de consumo |

**Brechas:**
- No hay reportes periódicos de consumo de alimentos por animal/lote

---

## 5. Gestión Sanitaria

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-013 | Registrar vacunas (próxima dosis) | ❌ | `EventoSanitario` genérico no diferencia vacunas ni tiene `proximaDosis` |
| RF-014 | Registrar tratamientos médicos | ✅ | Diagnóstico, tratamiento, severidad, observaciones |

**Brechas:**
- No hay un tipo específico `Vacuna` con campo de próxima fecha
- No hay alerta de vacunas próximas a vencer

---

## 6. Gestión Financiera

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-015 | Registrar costos y gastos | ✅ | `CostoOperativo` con categorías `ManoDeObra` / `CIF` |
| RF-016 | Clasificar gastos | ❌ | No existen las categorías `CostosDirectos`, `CostosIndirectos`, `GastosOperativos`, `Inversiones` |
| RF-017 | Proyección financiera | ⚠️ | `rentabilidadProyectada` por animal; **no hay flujo de caja ni proyección global** |
| RF-018 | Registrar préstamos | ❌ | No existe modelo de créditos, cuotas, intereses |

**Brechas:**
- Sistema de categorización de gastos insuficiente
- No hay reportes de estado de resultados, flujo de caja ni rentabilidad global
- No hay módulo de préstamos

---

## 7. Gestión de Producción

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-019 | Registrar producción agrícola | ❌ | No hay modelo para caña, pasto, silo |
| RF-020 | Etapas de inversión | ❌ | No hay concepto de proyecto/etapa/presupuesto |

**Brechas:**
- Módulo agrícola completamente ausente

---

## 8. Gestión de Mano de Obra

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-021 | Registrar empleados | ❌ | No existe entidad `Empleado` |
| RF-022 | Actividades realizadas | ❌ | No existe registro de actividades por empleado |

**Brechas:**
- No hay modelo de recursos humanos

---

## 9. Gestión de Subastas y Mercado

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-023 | Precios de mercado | ❌ | No existe registro histórico de precios por kg/tipo |
| RF-024 | Lotes de subasta | ❌ | No existe |

**Brechas:**
- No hay integración con precios de referencia del mercado

---

## 10. Reportes y Estadísticas

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-025 | Reportes de crecimiento | ✅ | Endpoints de analítica por animal y por lote |
| RF-026 | Reportes financieros | ❌ | No hay reportes agregados de gastos/ingresos/utilidad |
| RF-027 | Reportes sanitarios | ❌ | No hay consulta de vacunas pendientes ni historial exportable |

**Brechas:**
- Falta un módulo de reportes exportables (PDF, Excel)
- No hay reportes financieros ni sanitarios

---

## 11. Alertas y Notificaciones

| RF | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| RF-028 | Vacunas pendientes | ❌ | No existe concepto de próxima dosis |
| RF-029 | Pérdida de peso | ✅ | Animales ineficientes detectados por GMD baja |
| RF-030 | Gastos elevados | ❌ | No hay presupuesto contra el cual comparar |

**Brechas:**
- No hay motor de notificaciones (in-app, email)
- No hay presupuestos

---

## Resumen General

| Área | Total RF | ✅ | ⚠️ | ❌ | Cobertura |
|------|----------|----|-----|-----|-----------|
| 1. Bovinos | 4 | 4 | 0 | 0 | 100 % |
| 2. Peso | 3 | 2 | 1 | 0 | 83 % |
| 3. Compras | 3 | 0 | 0 | 3 | 0 % |
| 4. Alimentación | 2 | 1 | 0 | 1 | 50 % |
| 5. Sanidad | 2 | 1 | 0 | 1 | 50 % |
| 6. Finanzas | 4 | 1 | 1 | 2 | 37 % |
| 7. Producción | 2 | 0 | 0 | 2 | 0 % |
| 8. Mano de obra | 2 | 0 | 0 | 2 | 0 % |
| 9. Subastas | 2 | 0 | 0 | 2 | 0 % |
| 10. Reportes | 3 | 1 | 0 | 2 | 33 % |
| 11. Alertas | 3 | 1 | 0 | 2 | 33 % |
| **Total** | **30** | **11** | **2** | **17** | **40 %** |

---

## Brechas Prioritarias

1. **Proveedores y compras** — modelo completo ausente
2. **Vacunas** — falta campo `proximaDosis` y alerta asociada
3. **Gráficos de evolución de peso** — no hay charts
4. **Reportes financieros** — no hay agregación de gastos/ingresos
5. **Producción agrícola** — caña, pasto, silo
6. **Empleados** — no hay registro de personas ni actividades
7. **Préstamos y créditos** — no existe
8. **Presupuestos** — no hay contra qué comparar gastos
9. **Subastas / precios de mercado** — no existe
