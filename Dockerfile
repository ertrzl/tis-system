# Üç mərhələ: 1) React build olunur, 2) .NET API build olunur, 3) yalnız kiçik ASP.NET runtime ilə işlədilir.
# SDK və mənbə kod son image-ə düşmür. React build-i API-nin wwwroot qovluğuna qoyulur, beləliklə bir konteyner
# həm API-ni, həm də səhifələri verir.

# ---- 1-ci mərhələ: frontend ----
FROM node:22-alpine AS frontend
WORKDIR /web
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# ---- 2-ci mərhələ: backend ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Əvvəlcə yalnız .csproj faylları: NuGet paketləri bu qatda yüklənir və .csproj dəyişməyincə Docker onu saxlayır.
COPY src/Tis.Api/Tis.Api.csproj src/Tis.Api/
COPY src/Tis.Infrastructure/Tis.Infrastructure.csproj src/Tis.Infrastructure/
COPY src/Tis.Persistence/Tis.Persistence.csproj src/Tis.Persistence/
COPY src/Core/Tis.Application/Tis.Application.csproj src/Core/Tis.Application/
COPY src/Core/Tis.Domain/Tis.Domain.csproj src/Core/Tis.Domain/
RUN dotnet restore src/Tis.Api/Tis.Api.csproj

COPY src/ src/
RUN dotnet publish src/Tis.Api/Tis.Api.csproj -c Release -o /app/publish --no-restore

# ---- 3-cü mərhələ: işə salmaq ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# React build-i wwwroot-a (API onu statik fayl kimi verir)
COPY --from=frontend /web/dist ./wwwroot

# Tətbiqin yazdığı qovluq: logs (Serilog gündəlik log faylları). Root olmayan istifadəçiyə məxsus yaradılır, çünki
# boş qovluğa qoşulan volume həmin qovluğun sahibini götürür: bu olmasa tətbiq log sətri yaza bilməzdi.
RUN mkdir -p /app/logs && chown -R $APP_UID:$APP_UID /app/logs

# Root olmayan "app" istifadəçisi (APP_UID Microsoft image-lərində var). Veb serveri heç vaxt root kimi işlətmə.
USER $APP_UID

# 8080 portu: root olmayan istifadəçi 80-də dinləyə bilməz. HTTPS üçün qarşıda reverse proxy lazımdır.
EXPOSE 8080

ENTRYPOINT ["dotnet", "Tis.Api.dll"]
