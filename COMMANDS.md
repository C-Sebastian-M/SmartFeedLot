# SmartFeedLot — Comandos de base de datos

## IMPORTANTE: Si ya tienes una migración anterior, hay que regenerarla

```powershell
# 1. Borrar la migración anterior (si existe)
dotnet ef migrations remove --project src/Feedlot.Infrastructure --startup-project src/Feedlot.API

# 2. Borrar la base de datos para empezar limpio
dotnet ef database drop --project src/Feedlot.Infrastructure --startup-project src/Feedlot.API --force

# 3. Crear la migración nueva
dotnet ef migrations add InitialCreate --project src/Feedlot.Infrastructure --startup-project src/Feedlot.API --output-dir Persistence/Migrations

# 4. Arrancar la API (aplica la migración y siembra admin automáticamente)
dotnet run --project src/Feedlot.API
```

## Si es la primera vez (sin migraciones previas)

```powershell
dotnet ef migrations add InitialCreate --project src/Feedlot.Infrastructure --startup-project src/Feedlot.API --output-dir Persistence/Migrations

dotnet run --project src/Feedlot.API
```

## Arrancar el frontend (en otra terminal)

```powershell
cd frontend
npm run dev
```

## Verificar que la BD está bien

```
GET http://localhost:5000/api/auth/diagnostico
```

Debe responder: { "conexion": "OK", "totalUsuarios": 1, "totalRoles": 3 }
