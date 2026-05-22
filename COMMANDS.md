# SmartFeedLot — Comandos de base de datos

## Crear la migración inicial (solo se hace una vez)

```bash
cd C:\Users\sebas\Downloads\SmartFeedLot

dotnet ef migrations add InitialCreate \
  --project src/Feedlot.Infrastructure \
  --startup-project src/Feedlot.API \
  --output-dir Persistence/Migrations
```

En Windows PowerShell usar ` (backtick) en lugar de \ para continuar línea:
```powershell
dotnet ef migrations add InitialCreate `
  --project src/Feedlot.Infrastructure `
  --startup-project src/Feedlot.API `
  --output-dir Persistence/Migrations
```

O en una sola línea:
```bash
dotnet ef migrations add InitialCreate --project src/Feedlot.Infrastructure --startup-project src/Feedlot.API --output-dir Persistence/Migrations
```

## Aplicar la migración manualmente (opcional — el app lo hace al arrancar)

```bash
dotnet ef database update --project src/Feedlot.Infrastructure --startup-project src/Feedlot.API
```

## Arrancar la API

```bash
dotnet run --project src/Feedlot.API
```

## Arrancar el frontend

```bash
cd frontend
npm run dev
```
