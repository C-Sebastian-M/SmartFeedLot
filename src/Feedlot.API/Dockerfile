# ─── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto primero para aprovechar la caché de capas Docker.
# Si solo cambia código fuente (no .csproj), el restore no se re-ejecuta.
COPY ["src/Feedlot.Domain/Feedlot.Domain.csproj", "src/Feedlot.Domain/"]
COPY ["src/Feedlot.Application/Feedlot.Application.csproj", "src/Feedlot.Application/"]
COPY ["src/Feedlot.Infrastructure/Feedlot.Infrastructure.csproj", "src/Feedlot.Infrastructure/"]
COPY ["src/Feedlot.API/Feedlot.API.csproj", "src/Feedlot.API/"]

RUN dotnet restore "src/Feedlot.API/Feedlot.API.csproj"

# Copiar el código fuente completo.
COPY . .

RUN dotnet build "src/Feedlot.API/Feedlot.API.csproj" -c Release -o /app/build

# ─── Stage 2: Publish ────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "src/Feedlot.API/Feedlot.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ─── Stage 3: Runtime (imagen mínima) ────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Usuario no-root por seguridad.
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# Directorio de logs con permisos correctos.
RUN mkdir -p /app/logs

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "Feedlot.API.dll"]
