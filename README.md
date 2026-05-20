# SmartFeedLot

Plataforma de gestión y analítica de feedlot bovino construida con **.NET 8**, **Clean Architecture** y **DDD** (Domain-Driven Design).

---

## Arquitectura

```
Feedlot.API         (Web API — entry point, controllers, middleware)
    |
    +---> Feedlot.Application  (CQRS con MediatR, AutoMapper, FluentValidation)
    |         |
    |         +---> Feedlot.Domain  (entidades, value objects, enums, interfaces, eventos)
    |
    +---> Feedlot.Infrastructure  (EF Core + PostgreSQL, JWT Identity, BCrypt, repositorios)
              |
              +---> Feedlot.Application
```

### Capas

| Capa | Responsabilidad | Dependencias |
|------|----------------|--------------|
| **Domain** | Entidades, Value Objects, Enums, Interfaces de repositorio, Eventos de dominio, Servicios de dominio | Ninguna |
| **Application** | Commands, Queries, Handlers, DTOs, Mappers (AutoMapper), Validators (FluentValidation), Behaviors (pipeline MediatR) | Domain |
| **Infrastructure** | EF Core DbContext, Repositorios concretos, Identity (JWT), Persistencia | Application |
| **API** | Controllers, Middleware (excepciones, Serilog), Configuración (Swagger, JWT, CORS) | Infrastructure |

---

## Requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para PostgreSQL)
- PowerShell 5.1+ o bash

---

## Inicio rápido

### 1. Levantar PostgreSQL

```bash
docker compose up -d postgres
```

Esto crea un contenedor PostgreSQL 16 con:
- Puerto: `5433` (mapeado desde `5432` interno)
- Base de datos: `smartfeedlot`
- Usuario: `feedlot_user`
- Contraseña: `feedlot_pass`

> **Nota:** El puerto externo es `5433` para evitar conflictos con otras instalaciones de PostgreSQL en la máquina host.

### 2. Ejecutar la API

```bash
dotnet run --project src/Feedlot.API
```

La API arranca en `http://localhost:5000` con Swagger en la raíz.

O con perfil de desarrollo (puertos HTTPS):

```bash
dotnet run --project src/Feedlot.API --launch-profile "Feedlot.API"
```

### 3. Probar los endpoints

```bash
# 1. Registrar usuario
curl -k -X POST https://localhost:53218/api/auth/registro \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@feedlot.com","nombreCompleto":"Admin Feedlot","password":"Password123!","rol":"Admin"}'

# 2. Login
curl -k -X POST https://localhost:53218/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@feedlot.com","password":"Password123!"}'

# 3. Usar el token en endpoints protegidos
curl -k -X POST https://localhost:53218/api/Animals \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"codigoIdentificacion":"AN-001","numeroArete":"AR-001","sexo":"Macho","raza":"Angus","fechaNacimiento":"2026-01-15","pesoIngresoKg":250.5,"precioCompra":1500000,"moneda":"COP","fechaIngreso":"2026-05-01","loteInicialId":null}'
```

### 4. Ver base de datos con pgAdmin

```bash
docker run -d --name pgadmin -p 5050:80 \
  -e PGADMIN_DEFAULT_EMAIL=admin@gmail.com \
  -e PGADMIN_DEFAULT_PASSWORD=admin \
  dpage/pgadmin4
```

Entrar a `http://localhost:5050` y registrar server con:
- Host: `host.docker.internal`
- Port: `5433`
- Database: `smartfeedlot`
- User: `feedlot_user`
- Password: `feedlot_pass`

---

## API — Endpoints

### Autenticación (pública)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/auth/registro` | Registra un nuevo usuario |
| `POST` | `/api/auth/login` | Login, devuelve JWT token |

### Animals (requiere Authorization: Bearer)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/Animals` | Lista paginada de animales (filtros por query params) |
| `GET` | `/api/Animals/{id}` | Detalle completo de un animal |
| `POST` | `/api/Animals` | Registra un nuevo animal |
| `POST` | `/api/Animals/{id}/pesajes` | Registra un pesaje en el animal |
| `POST` | `/api/Animals/{id}/eventos-sanitarios` | Registra un evento sanitario |
| `GET` | `/api/Analitica/animales/{id}/indicadores` | Indicadores productivos del animal |

### Lotes (requiere Authorization: Bearer)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/Lotes` | Lista de lotes (`?soloActivos=true`) |
| `GET` | `/api/Lotes/{id}` | Detalle del lote con sus animales |
| `POST` | `/api/Lotes` | Crea un nuevo lote |
| `POST` | `/api/Lotes/{id}/mover-animal` | Mueve un animal a otro lote |
| `GET` | `/api/Analitica/lotes/{id}/resumen` | Resumen de indicadores del lote |

### Nutrición (requiere Authorization: Bearer)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/Nutricion/raciones` | Crea una nueva ración |
| `POST` | `/api/Nutricion/consumos` | Registra consumo diario de un lote |

### Analítica (requiere Authorization: Bearer)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/Analitica/animales/{id}/indicadores` | GMD, ICA, costo/kg, rentabilidad |
| `GET` | `/api/Analitica/lotes/{id}/resumen` | Dashboard consolidado del lote |
| `GET` | `/api/Analitica/animales-ineficientes` | Animales fuera de umbrales productivos |

---

## Modelos de datos (JSON)

### Auth

**Registro**
```json
{
  "email": "admin@feedlot.com",
  "nombreCompleto": "Admin Feedlot",
  "password": "Password123!",
  "rol": "Admin"
}
```

**Login**
```json
{
  "email": "admin@feedlot.com",
  "password": "Password123!"
}
```

### Registrar Animal
```json
{
  "codigoIdentificacion": "AN-001",
  "numeroArete": "AR-001",
  "sexo": "Macho",
  "raza": "Angus",
  "fechaNacimiento": "2026-01-15",
  "pesoIngresoKg": 250.5,
  "precioCompra": 1500000,
  "moneda": "COP",
  "fechaIngreso": "2026-05-01",
  "loteInicialId": null
}
```

### Registrar Pesaje
```json
{
  "fechaPesaje": "2026-05-20",
  "pesoKg": 320.0,
  "observaciones": null
}
```

### Registrar Evento Sanitario
```json
{
  "fechaEvento": "2026-05-20",
  "diagnostico": "Pododermatitis",
  "descripcion": "Cojera en miembro posterior derecho",
  "severidad": "Moderado",
  "tratamiento": "Antibiótico tópico por 5 días"
}
```

### Crear Lote
```json
{
  "codigo": "L-001",
  "nombre": "Lote de Prueba",
  "capacidadMaxima": 100
}
```

### Mover Animal a Lote
```json
{
  "animalId": "guid-del-animal",
  "fechaMovimiento": "2026-05-20",
  "motivo": "Reclasificacion"
}
```

### Crear Ración
```json
{
  "nombre": "Ración Estándar",
  "costoKg": 1200.50,
  "moneda": "COP",
  "proteinaPct": 16.5,
  "energiaMcal": 2.8
}
```

### Registrar Consumo
```json
{
  "loteId": "guid-del-lote",
  "racionId": "guid-de-la-racion",
  "fecha": "2026-05-20",
  "cantidadKg": 150.0,
  "costoTotal": 180000.0,
  "moneda": "COP",
  "registradoPorId": "guid-del-usuario"
}
```

---

## Dominio — Entities y Value Objects

### Aggregate Roots

| Entidad | Descripción |
|---------|-------------|
| `Animal` | Bovino individual. Contiene `Pesaje[]` y `EventoSanitario[]` |
| `Lote` | Grupo de animales en engorde colectivo. Contiene `AnimalLote[]` |
| `ConsumoAlimenticio` | Registro diario de alimento suministrado a un lote |
| `Racion` | Fórmula alimenticia con valores nutricionales. Contiene `RacionIngrediente[]` |
| `Ingrediente` | Insumo alimenticio con perfil nutricional |

### Value Objects

| Value Object | Uso |
|---|---|
| `CodigoIdentificacion` | Código alfanumérico del animal (caravana/arete) |
| `Peso` | Peso en kg, validado > 0 |
| `Dinero` | Monto + moneda (ISO 4217). Previene mezclar monedas |
| `Capacidad` | Capacidad del lote (máximo/actual) |
| `CantidadKilogramos` | Cantidad de alimento en kg, no negativa |
| `IndicadorProductivo` | GMD, ICA, costo/kg, rentabilidad proyectada |

### Enumeraciones

| Enum | Valores |
|------|---------|
| `Sexo` | `Macho`, `Hembra` |
| `EstadoProductivo` | `EnEngorde`, `Vendido`, `Muerto`, `Retirado` |
| `EstadoSanitario` | `Sano`, `EnTratamiento`, `EnRetiro`, `Restringido` |
| `EstadoLote` | `Activo`, `Cerrado`, `EnPreparacion` |
| `MotivoMovimiento` | `IngresoInicial`, `Reclasificacion`, `Sanitario`, `Capacidad`, `Venta`, `Muerte`, `Otro` |
| `SeveridadEvento` | `Leve`, `Moderado`, `Grave`, `Critico` |

### Domain Events

| Evento | Cuándo se emite |
|--------|----------------|
| `AnimalRegistradoEvent` | Nuevo animal creado |
| `PesajeRegistradoEvent` | Nuevo pesaje registrado |
| `ConsumoAlimenticioRegistradoEvent` | Nuevo consumo registrado |
| `AnimalMovidoALoteEvent` | Animal movido entre lotes |
| `EventoSanitarioRegistradoEvent` | Evento sanitario registrado |

---

## CQRS — Pipeline MediatR

El pipeline de MediatR ejecuta estos behaviors en orden:

```
Request → [LoggingBehavior] → [ValidationBehavior] → [UnitOfWorkBehavior] → Handler
```

1. **LoggingBehavior** — log de entrada/salida de cada request
2. **ValidationBehavior** — ejecuta todos los `FluentValidation` validators; lanza `400` si falla
3. **UnitOfWorkBehavior** — llama `SaveChangesAsync` automáticamente después del handler
4. **Handler** — contiene la lógica de aplicación (orquesta repositorios + dominio)

---

## Reglas de validación (FluentValidation)

### RegistrarAnimalCommand
| Campo | Reglas |
|-------|--------|
| `codigoIdentificacion` | Requerido, 3-20 caracteres, solo alfanumérico + guiones |
| `numeroArete` | Requerido, máx 50 caracteres |
| `sexo` | `Macho` o `Hembra` |
| `raza` | Requerido, máx 100 caracteres |
| `fechaNacimiento` | Requerido, debe ser anterior a hoy |
| `pesoIngresoKg` | 0 - 2000 kg |
| `precioCompra` | No negativo |
| `moneda` | `COP`, `USD` o `EUR` |
| `fechaIngreso` | Debe ser posterior a fecha de nacimiento |

### CrearLoteCommand
| Campo | Reglas |
|-------|--------|
| `codigo` | Requerido, máx 20, alfanumérico + guiones |
| `nombre` | Requerido, máx 100 caracteres |
| `capacidadMaxima` | 1 - 10000 |

### MoverAnimalALoteCommand
| Campo | Reglas |
|-------|--------|
| `animalId` | Requerido |
| `loteDestinoId` | Requerido, no igual a animalId |
| `fechaMovimiento` | Requerido, ≤ hoy |
| `motivo` | Requerido, debe ser enum válido: `IngresoInicial`, `Reclasificacion`, `Sanitario`, `Capacidad`, `Venta`, `Muerte`, `Otro` |

### CrearRacionCommand
| Campo | Reglas |
|-------|--------|
| `nombre` | Requerido, máx 150 |
| `costoKg` | > 0 |
| `moneda` | Requerido, 3 caracteres |
| `proteinaPct` | 0 - 100 |
| `energiaMcal` | ≥ 0 |

### RegistrarConsumoCommand
| Campo | Reglas |
|-------|--------|
| `loteId` | Requerido |
| `racionId` | Requerido |
| `fecha` | Requerido, ≤ hoy |
| `cantidadKg` | ≥ 0 |
| `costoTotal` | ≥ 0 |
| `moneda` | Requerido, 3 caracteres |
| `registradoPorId` | Requerido |

### RegistrarPesajeCommand
| Campo | Reglas |
|-------|--------|
| `animalId` | Requerido |
| `fechaPesaje` | Requerido, ≤ hoy |
| `pesoKg` | 0 - 2000 |
| `observaciones` | Máx 500 caracteres |

### RegistrarEventoSanitarioCommand
| Campo | Reglas |
|-------|--------|
| `animalId` | Requerido |
| `fechaEvento` | Requerido, ≤ hoy |
| `diagnostico` | Requerido, máx 200 |
| `descripcion` | Requerido, máx 1000 |
| `severidad` | `Leve`, `Moderado`, `Grave`, `Critico` |
| `tratamiento` | Máx 500 |

---

## Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=smartfeedlot;Username=feedlot_user;Password=feedlot_pass;Include Error Detail=true"
  },
  "JwtSettings": {
    "SecretKey": "CAMBIAR-EN-PRODUCCION-MIN-32-CARACTERES-CLAVE-SECRETA",
    "Issuer": "SmartFeedLot",
    "Audience": "SmartFeedLot-Frontend",
    "ExpirationMinutes": 480
  }
}
```

### docker-compose.yml

```yaml
services:
  postgres:
    image: postgres:16-alpine
    container_name: feedlot_postgres
    environment:
      POSTGRES_DB: smartfeedlot
      POSTGRES_USER: feedlot_user
      POSTGRES_PASSWORD: feedlot_pass
    ports:
      - "5433:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U feedlot_user -d smartfeedlot"]

  api:
    build: .
    container_name: feedlot_api
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:5000
      ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=smartfeedlot;Username=feedlot_user;Password=feedlot_pass
    ports:
      - "5000:5000"
```

---

## Indicadores productivos

### GMD — Ganancia Media Diaria
```
GMD = (Peso Final - Peso Inicial) / Días en Engorde
```

### ICA — Índice de Conversión Alimenticia
```
ICA = Alimento Consumido (kg) / Peso Ganado (kg)
```

### Costo por kg ganado
```
Costo/kg = Costo Total Alimento / Peso Ganado
```

### Rentabilidad Proyectada
```
Rentabilidad = Precio Venta Estimado - Costos Totales
```

---

## Solución de problemas comunes

### "password authentication failed for user feedlot_user"
El host ya tiene PostgreSQL instalado en el puerto `5432`. El Docker PostgreSQL usa `5433`. Verificar que la conexión apunte al puerto correcto.

### "Missing partial modifier"
Agregar `partial` a la clase `FeedlotDbContext`:
```csharp
public sealed partial class FeedlotDbContext : DbContext, IUnitOfWork
```

### Paquete Microsoft.Extensions.Options downgrade
Sincronizar versiones entre proyectos. `Feedlot.Infrastructure` debe usar v10 para coincidir con `AutoMapper 16.1.1`.

---

## Tecnologías

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8 + Npgsql (PostgreSQL)
- MediatR (CQRS)
- FluentValidation
- AutoMapper
- JWT Bearer Authentication + BCrypt
- Serilog (file + console)
- Swagger / OpenAPI
- Docker + docker-compose
