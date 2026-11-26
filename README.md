# Tələbə İnformasiya Sistemi (TİS)

Azərbaycan universitetləri üçün çoxkurumlu (multi-tenant) tələbə məlumat sistemi. Portfel layihəsi.

- **Backend:** ASP.NET Core (net10.0) + EF Core + SQL Server
- **Frontend:** React + TypeScript (Vite)
- **Dillər:** Azərbaycan dili (əsas), İngilis və Rus dilləri

## Qovluq strukturu

```
tis-system/
  Tis.slnx
  Dockerfile, docker-compose.yml, .env.example, .dockerignore, start-dev.ps1
  .github/workflows/ci.yml
  frontend/                      React (Vite)
    src/api, auth, components, pages
    src/i18n/locales/            az.json, en.json, ru.json (UI mətnləri)
  src/
    Core/
      Tis.Domain/                Common (BaseEntity), Entities, Enums
      Tis.Application/           Interfaces (Repositories, Services), DTOs, Validators, Exceptions,
                                 MappingProfiles, Common (Loc, PagedResult), Locales/*.json (API mesajları)
    Tis.Persistence/             TisDbContext, Configurations, Converters, Migrations,
                                 Implementations (Repositories/Generic, Services)
    Tis.Infrastructure/          JWT və token xidməti, CurrentUser, Email (Smtp/Console/Disabled)
    Tis.Api/                     Controllers, Middlewares, Filters, Extensions, Program.cs
```

Asılılıq istiqaməti: `Api → Persistence, Infrastructure → Application → Domain`.

## Hazır altyapı

| Mövzu | Necə işləyir |
|-------|--------------|
| Çoxuniversitetlilik | Hər qeyd `TenantId` daşıyır. `TisDbContext` hər sorğunu avtomatik hazırkı istifadəçinin universitetinə görə süzür və yeni qeydə `TenantId` yazır |
| Giriş | Universitet kodu + (tələbə nömrəsi **və ya** e-poçt) + parol. JWT giriş tokeni (15 dəq) + yenilənmə tokeni (30 gün) |
| Yenilənmə tokeni | Bazada yalnız SHA-256 hash-i. Hər istifadədə yenisi ilə əvəz olunur. Köhnəsi təkrar gəlsə, istifadəçinin bütün sessiyaları ləğv olunur |
| Hesab kilidi | Ardıcıl 5 uğursuz parol cəhdindən sonra 15 dəqiqə bağlanır (`Auth` ayarları) |
| Rate limiting | Giriş/yeniləmə/çıxış: ünvana görə 20/dəq. Qalan sorğular: 600/dəq ("RateLimiting" ayarları) |
| Doğrulama | FluentValidation, hər DTO üçün bir doğrulayıcı. Xətalar 400 + üç dildə mesaj |
| Xəta idarəsi | `BaseException` → HTTP statusu (400/401/403/404/409/429). Gözlənilməyən xəta ümumi 500 olur |
| Loglama | Serilog: konsol + `logs/app-.log` (gündəlik, 14 gün). Hər sətirdə istifadəçi və IP |
| Təhlükəsizlik | Təhlükəsizlik başlıqları, `Server` başlığı yoxdur, CSP (Development-dən kənarda), `/api` üçün `no-store` |
| Çoxdillilik | API mesajları `Loc.T("açar")` ilə `Locales/*.json`-dan, `Accept-Language`-ə görə. Yeni dil = yeni JSON faylı |
| Verilənlər | Ümumi `IRepository<T>`, `BaseEntity` (CreatedAt/UpdatedAt avtomatik), tarixlər UTC, `PagedResult<T>`, AutoMapper |
| E-poçt | `IEmailSender`: SMTP ilə (MailKit). SMTP yoxdursa Development-də konsola yazılır, digərində atılır |
| Arxa plan | `MaintenanceBackgroundService`: köhnə yenilənmə tokenlərini təmizləyir |
| Sağlamlıq | `GET /health` (verilənlər bazası cavab verirsə 200). Swagger: `/swagger` (yalnız Development) |

Hələ yoxdur (lazım olanda əlavə olunacaq): fayl yükləmə, parol sıfırlama/dəvət axını, real zamanlı bildirişlər.

## Lokalda işə salmaq

1. SQL Server Express işləməlidir (`MSSQL$SQLEXPRESS` servisi). Bağlantı sətri `src/Tis.Api/appsettings.json` içindədir.
2. Verilənlər bazasını yaradın:

```powershell
dotnet tool restore
dotnet ef database update -p src/Tis.Persistence -s src/Tis.Api
```

3. `src/Tis.Api/appsettings.Development.json` faylını yaradın (git-ə düşmür, gizli məlumat saxlayır):

```json
{
  "Jwt": { "SecretKey": "ən-azı-32-simvolluq-təsadüfi-mətn" },
  "Seed": {
    "TenantCode": "DEMO",
    "AdminEmail": "admin@demo.az",
    "AdminPassword": "öz-parolunuz",
    "DemoStudentNumber": "20240001",
    "DemoStudentEmail": "student@demo.az"
  }
}
```

   `Seed:AdminPassword` təyin olunubsa tətbiq başlayanda ilk universitet və admin yaradılır. `DemoStudent*` verilsə,
   eyni parolla bir test tələbəsi də yaradılır.

4. İşə salın: Visual Studio-da yaşıl düymə (API ilə birlikdə Vite də açılır və brauzer açılır) və ya

```powershell
.\start-dev.ps1
```

Yeni migration: `dotnet ef migrations add <Ad> -p src/Tis.Persistence -s src/Tis.Api -o Contexts/Migrations`

## Docker ilə

`.env.example` faylını `.env` kimi kopyalayın, `SA_PASSWORD` və `JWT_SECRET_KEY` təyin edin, sonra `docker compose up --build`
və http://localhost:8080 ünvanını açın.

## 1 aylıq plan

| Həftə | Hədəf |
|-------|-------|
| 1 | Altyapı (hazır), giriş, **SuperAdmin və universitet yaratma**, istifadəçi əlavə etmə və dəvət axını |
| 2 | Akademik struktur (fakültə, kafedra, ixtisas, qrup), fənlər, semestr, dərs cədvəli |
| 3 | Davamiyyət, qiymətlər (50+50 balla, qəbul həddi ayarlana bilən), tələbə və müəllim panelləri |
| 4 | Transkript PDF, elanlar, testlər, deploy, ekran görüntüləri |
