# Prompt para Stitch — Frontend SmartFeedLot

## Descripción del proyecto

Plataforma **web** de gestión y analítica de **feedlot bovino**. Backend en .NET 8 con Clean Architecture + DDD. Se necesita un frontend SPA (React/Vite + TypeScript) que consuma la API REST.

La API corre en `http://localhost:53219` (desarrollo) con JWT Bearer authentication. CORS ya está configurado para `http://localhost:5173` (Vite).

---

## Requisitos técnicos

- **Framework:** React 18+ con TypeScript
- **Build:** Vite
- **UI:** Libre (TailwindCSS + shadcn/ui recomendado, o Material UI)
- **HTTP Client:** Axios o fetch
- **Routing:** React Router v6
- **Formularios:** React Hook Form + Zod (validación)
- **Gráficos:** Recharts o Chart.js
- **Estado:** Context API o Zustand (simple)
- **Layout responsive** (mobile-first, funciona en tablet/escritorio)

---

## Páginas / Vistas requeridas

### 1. Login / Registro
- Formulario de login (email + password)
- Formulario de registro (email, nombre completo, password, rol)
- Botón de "Registrarse" / "Iniciar Sesión"
- Guardar JWT token en localStorage y enviarlo en cada request (`Authorization: Bearer <token>`)
- Redirigir al dashboard después del login

### 2. Dashboard principal
- **Cards resumen:** Total animales, lotes activos, animales ineficientes, GMD promedio general
- **Gráfico:** GMD por lote (barras)
- **Tabla:** Últimos 5 animales ineficientes
- **Sidebar** con navegación: Dashboard, Animales, Lotes, Nutrición, Analítica

### 3. Animales (CRUD + eventos)
- **Lista:** Tabla paginada con columnas: código, arete, raza, sexo, peso actual, días en engorde, estado productivo, estado sanitario
- **Filtros:** por estado productivo, estado sanitario, raza, búsqueda por código/arete
- **Crear animal:** Formulario con todos los campos del POST
- **Detalle animal:** Info general + timeline de pesajes + timeline de eventos sanitarios
- **Registrar pesaje:** Formulario inline en el detalle
- **Registrar evento sanitario:** Formulario inline en el detalle
- **Indicadores:** Sección con GMD, ICA, costo/kg, rentabilidad (usar GET /api/Analitica/animales/{id}/indicadores)

### 4. Lotes
- **Lista:** Tarjetas mostrando código, nombre, capacidad (actual/máxima), % ocupación, estado
- **Crear lote:** Formulario
- **Detalle lote:** Tabla de animales en el lote + botón "Mover animal"
- **Mover animal:** Modal con selector de animal + fecha + motivo
- **Resumen lote:** Sección con GMD promedio, ICA promedio, rentabilidad total (GET /api/Analitica/lotes/{id}/resumen)

### 5. Nutrición
- **Raciones:** Lista + formulario de creación
- **Consumos:** Formulario para registrar consumo diario (selector de lote, ración, fecha, cantidad, costo)
- **Tabla histórica** de consumos por lote

### 6. Analítica
- **Animales ineficientes:** Tabla con animales donde GMD < umbral o ICA > umbral, con filtros por lote, fechas, umbrales configurables
- **Gráficos:** GMD por animal (barras), ICA por lote (comparativa)
- Exportar a CSV (opcional)

---

## API Reference

### Base URL
```
http://localhost:53219/api
```

### Autenticación

**Registro** `POST /api/auth/registro` (público)
```json
// Request
{"email":"admin@feedlot.com","nombreCompleto":"Admin Feedlot","password":"Password123!","rol":"Admin"}

// Response 201
{"id":"guid","email":"admin@feedlot.com","nombre":"Admin Feedlot"}
```

**Login** `POST /api/auth/login` (público)
```json
// Request
{"email":"admin@feedlot.com","password":"Admin123!"}

// Response 200
{"token":"jwt...","usuario":{"id":"guid","email":"...","nombre":"...","roles":["Admin"]},"expiraEn":"2026-...Z"}
```

> **⚠️ Credenciales por defecto:** admin@feedlot.com / Admin123!

---

### Animals (requieren `Authorization: Bearer <token>`)

**Listar** `GET /api/Animals?page=1&pageSize=20&estadoProductivo=&estadoSanitario=&raza=&busqueda=`
```json
// Response
{
  "items": [
    {
      "id": "guid",
      "codigoIdentificacion": "AN-001",
      "numeroArete": "AR-001",
      "raza": "Angus",
      "sexo": "Macho",
      "pesoActualKg": 320.0,
      "diasEnEngorde": 20,
      "estadoProductivo": "EnEngorde",
      "estadoSanitario": "Sano"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

**Detalle** `GET /api/Animals/{id}`
```json
{
  "id": "guid",
  "codigoIdentificacion": "AN-001",
  "numeroArete": "AR-001",
  "sexo": "Macho",
  "raza": "Angus",
  "fechaNacimiento": "2026-01-15",
  "pesoIngresoKg": 250.5,
  "precioCompra": 1500000.0,
  "moneda": "COP",
  "fechaIngreso": "2026-05-01",
  "estadoProductivo": "EnEngorde",
  "estadoSanitario": "Sano",
  "pesoActualKg": 320.0,
  "diasEnEngorde": 20,
  "totalPesajes": 2
}
```

**Crear** `POST /api/Animals`
```json
{
  "codigoIdentificacion": "AN-001",
  "numeroArete": "AR-001",
  "sexo": "Macho",
  "raza": "Angus",
  "fechaNacimiento": "2026-01-15",
  "pesoIngresoKg": 250.5,
  "precioCompra": 1500000.0,
  "moneda": "COP",
  "fechaIngreso": "2026-05-01",
  "loteInicialId": null
}
```
→ Response `201 Created` con `{"id": "guid", ...}`

**Registrar Pesaje** `POST /api/Animals/{id}/pesajes`
```json
{"fechaPesaje":"2026-05-20","pesoKg":320.0,"observaciones":null}
```

**Registrar Evento Sanitario** `POST /api/Animals/{id}/eventos-sanitarios`
```json
{"fechaEvento":"2026-05-20","diagnostico":"Pododermatitis","descripcion":"Cojera en miembro posterior derecho","severidad":"Moderado","tratamiento":"Antibiótico tópico por 5 días"}
```

> `severidad` acepta: `Leve`, `Moderado`, `Grave`, `Critico`

---

### Lotes (requieren `Authorization: Bearer <token>`)

**Listar** `GET /api/Lotes?soloActivos=true`
```json
[
  {
    "id": "guid",
    "codigo": "L-001",
    "nombre": "Lote de Prueba",
    "capacidadMaxima": 100,
    "animalesActuales": 5,
    "porcentajeOcupacion": 5.0,
    "estado": "Activo"
  }
]
```

**Detalle** `GET /api/Lotes/{id}`
```json
{
  "id": "guid",
  "codigo": "L-001",
  "nombre": "Lote de Prueba",
  "capacidadMaxima": 100,
  "animalesActuales": 5,
  "porcentajeOcupacion": 5.0,
  "estado": "Activo",
  "animales": [
    {
      "animalId": "guid",
      "codigoAnimal": "AN-001",
      "fechaIngreso": "2026-05-01",
      "fechaEgreso": null,
      "motivoIngreso": "IngresoInicial",
      "esActivo": true,
      "diasEnLote": 20
    }
  ]
}
```

**Crear** `POST /api/Lotes`
```json
{"codigo":"L-001","nombre":"Lote de Prueba","capacidadMaxima":100}
```
→ Response `201 Created`

**Mover Animal** `POST /api/Lotes/{loteDestinoId}/mover-animal`
```json
{"animalId":"guid-del-animal","fechaMovimiento":"2026-05-20","motivo":"Reclasificacion"}
```
> `motivo` acepta: `IngresoInicial`, `Reclasificacion`, `Sanitario`, `Capacidad`, `Venta`, `Muerte`, `Otro`

---

### Nutrición (requieren `Authorization: Bearer <token>`)

**Crear Ración** `POST /api/Nutricion/raciones`
```json
{"nombre":"Ración Estándar","costoKg":1200.50,"moneda":"COP","proteinaPct":16.5,"energiaMcal":2.8}
```
> `moneda` acepta: `COP`, `USD`, `EUR`

**Registrar Consumo** `POST /api/Nutricion/consumos`
```json
{"loteId":"guid","racionId":"guid","fecha":"2026-05-20","cantidadKg":150.0,"costoTotal":180000.0,"moneda":"COP","registradoPorId":"guid-del-usuario"}
```

---

### Analítica (requieren `Authorization: Bearer <token>`)

**Indicadores de animal** `GET /api/Analitica/animales/{animalId}/indicadores?loteId=guid&desde=2026-01-01&hasta=2026-05-20&precioVentaEstimadoPorKg=5500`
```json
{
  "animalId": "guid",
  "codigoAnimal": "AN-001",
  "raza": "Angus",
  "pesoInicialKg": 250.5,
  "pesoActualKg": 320.0,
  "pesoGanadoKg": 69.5,
  "diasEnEngorde": 20,
  "gmd": 3.475,
  "ica": 2.15,
  "costoPorKgGanado": 21586.0,
  "rentabilidadProyectada": 500000.0,
  "esIneficiente": false,
  "clasificacionGmd": "Excelente"
}
```

**Resumen de lote** `GET /api/Analitica/lotes/{loteId}/resumen?desde=2026-01-01&hasta=2026-05-20&precioVentaEstimadoPorKg=5500`
```json
{
  "loteId": "guid",
  "codigoLote": "L-001",
  "nombreLote": "Lote de Prueba",
  "totalAnimales": 5,
  "gmdPromedioKgDia": 2.8,
  "icaPromedio": 3.1,
  "costoTotalAlimento": 180000.0,
  "costoPorKgGanadoPromedio": 21586.0,
  "rentabilidadProyectadaTotal": 2500000.0,
  "animalesIneficientes": 1,
  "consumoTotalKg": 1500,
  "indicadores": [/* array de IndicadorProductivoDto */]
}
```

**Animales ineficientes** `GET /api/Analitica/animales-ineficientes?loteId=&desde=2026-01-01&hasta=2026-05-20&precioVentaEstimadoPorKg=5500&gmdMinima=0.8&icaMaxima=8`
```json
[
  {
    "animalId": "guid",
    "codigoAnimal": "AN-005",
    "raza": "Angus",
    "loteCodigo": "L-001",
    "gmd": 0.5,
    "gmdMinimaEsperada": 0.8,
    "ica": 9.2,
    "icaMaximaEsperada": 8.0,
    "diasEnEngorde": 30,
    "motivoAlerta": "GMD por debajo del umbral (0.5 < 0.8)"
  }
]
```

---

## Enums (para selects/dropdowns)

| Campo | Valores |
|-------|---------|
| `sexo` | `Macho`, `Hembra` |
| `moneda` | `COP`, `USD`, `EUR` |
| `estadoProductivo` | `EnEngorde`, `Vendido`, `Muerto`, `Retirado` |
| `estadoSanitario` | `Sano`, `EnTratamiento`, `EnRetiro`, `Restringido` |
| `estadoLote` | `Activo`, `Cerrado`, `EnPreparacion` |
| `motivoMovimiento` | `IngresoInicial`, `Reclasificacion`, `Sanitario`, `Capacidad`, `Venta`, `Muerte`, `Otro` |
| `severidad` | `Leve`, `Moderado`, `Grave`, `Critico` |
| `rol` | `Admin`, `Supervisor`, `Operador` |

---

## Reglas de validación (útil para forms)

### Registrar animal
- `codigoIdentificacion`: requerido, 3-20 chars, alfanumérico + guiones
- `numeroArete`: requerido, máx 50
- `sexo`: `Macho` o `Hembra`
- `raza`: requerido, máx 100
- `fechaNacimiento`: obligatoria, anterior a hoy
- `pesoIngresoKg`: > 0 y < 2000
- `precioCompra`: ≥ 0
- `moneda`: `COP`, `USD` o `EUR`
- `fechaIngreso`: posterior a fechaNacimiento
- `loteInicialId`: opcional (null o GUID de lote existente)

### Crear lote
- `codigo`: requerido, máx 20, alfanumérico + guiones
- `nombre`: requerido, máx 100
- `capacidadMaxima`: 1-10000

### Ración
- `nombre`: requerido, máx 150
- `costoKg`: > 0
- `moneda`: requerido, 3 chars
- `proteinaPct`: 0-100
- `energiaMcal`: ≥ 0

### Consumo
- `loteId`, `racionId`, `registradoPorId`: GUIDs requeridos
- `fecha`: ≤ hoy
- `cantidadKg`: ≥ 0
- `costoTotal`: ≥ 0
- `moneda`: requerido, 3 chars

---

## Manejo de errores

La API devuelve:

- **200/201** — éxito
- **400** — error de validación (`ValidationException` de FluentValidation con detalles de campos)
- **401** — no autenticado (sin token o token inválido/expirado)
- **404** — recurso no encontrado
- **409** — conflicto (email duplicado, código duplicado)
- **422** — error de dominio (regla de negocio violada)

El formato de error 400:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Nombre": ["El nombre es requerido."]
  }
}
```

El formato de error 422:
```json
{
  "error": "El animal ya está registrado como muerto."
}
```

---

## Notas de diseño UX

1. **Paleta de colores:** Tonos verdes (ganadería/campo) + azul profesional. Blanco/neutro de fondo.
2. Los **enums deben mostrarse en español amigable** (ej. "EnEngorde" → "En engorde", "Leve" → "Leve")
3. Los **DateOnly** se envían/reciben como string ISO: `"2026-05-20"`
4. Los **decimales** se envían como número: `250.5`
5. Los campos de GUIDs deben usar **selectores** (dropdown de lotes, raciones, usuarios), no inputs libres
6. **Todas las tablas deben ser paginadas** (el backend soporta `?page=1&pageSize=20`)
7. Mostrar **notificaciones toast** para éxito/error de operaciones

---

## Estructura de archivos sugerida

```
src/
├── api/
│   ├── client.ts              # Axios instance con interceptor JWT
│   ├── auth.ts                # login(), registro()
│   ├── animals.ts             # listar(), obtener(), crear(), registrarPesaje(), etc.
│   ├── lotes.ts               # listar(), obtener(), crear(), moverAnimal()
│   ├── nutricion.ts           # crearRacion(), registrarConsumo()
│   └── analitica.ts           # indicadores(), resumenLote(), ineficientes()
├── components/
│   ├── ui/                    # shadcn/ui components (button, input, card, table, etc.)
│   ├── layout/
│   │   ├── Sidebar.tsx
│   │   ├── Navbar.tsx
│   │   └── AppLayout.tsx
│   ├── auth/
│   │   ├── LoginForm.tsx
│   │   └── RegistroForm.tsx
│   ├── animals/
│   │   ├── AnimalList.tsx
│   │   ├── AnimalForm.tsx
│   │   ├── AnimalDetail.tsx
│   │   ├── PesajeForm.tsx
│   │   └── EventoSanitarioForm.tsx
│   ├── lotes/
│   │   ├── LoteList.tsx
│   │   ├── LoteForm.tsx
│   │   ├── LoteDetail.tsx
│   │   └── MoverAnimalModal.tsx
│   ├── nutricion/
│   │   ├── RacionForm.tsx
│   │   └── ConsumoForm.tsx
│   └── analitica/
│       ├── IndicadoresCard.tsx
│       ├── ResumenLoteCard.tsx
│       └── IneficientesTable.tsx
├── pages/
│   ├── LoginPage.tsx
│   ├── RegisterPage.tsx
│   ├── DashboardPage.tsx
│   ├── AnimalsPage.tsx
│   ├── AnimalDetailPage.tsx
│   ├── LotesPage.tsx
│   ├── LoteDetailPage.tsx
│   ├── NutricionPage.tsx
│   └── AnaliticaPage.tsx
├── hooks/
│   ├── useAuth.ts
│   └── useApi.ts
├── lib/
│   ├── utils.ts               # cn() de tailwind
│   └── constants.ts           # enums, colores
├── context/
│   └── AuthContext.tsx
├── types/
│   └── index.ts               # interfaces TypeScript de todos los DTOs
├── App.tsx
└── main.tsx
```
