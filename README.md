# Tələbə İnformasiya Sistemi (TİS)

Azərbaycan universitetləri üçün çoxkurumlu (multi-tenant) tələbə məlumat sistemi. Portfel layihəsi.

- **Backend:** ASP.NET Core (net10.0) + EF Core + SQL Server
- **Frontend:** React + TypeScript (Vite)
- **Dillər:** Azərbaycan dili (əsas), İngilis və Rus dilləri

## Qovluq strukturu

```
tis-system/
  Tis.slnx
  Dockerfile, docker-compose.yml, .env.example, .dockerignore
  .github/workflows/ci.yml
  frontend/                      React (Vite)
    src/i18n/locales/            az.json, en.json, ru.json (UI mətnləri)
  src/
    Core/
      Tis.Domain/                Entities, Enums
      Tis.Application/           Interfaces, DTOs, Validators, Exceptions, MappingProfiles,
                                 Common (Loc), Locales/*.json (API mesajları)
    Tis.Persistence/             TisDbContext, Configurations, Migrations, Repositories, Services
    Tis.Infrastructure/          Email, JWT və digər xarici xidmətlər
    Tis.Api/                     Controllers, Middlewares, Filters, Program.cs
```

Asılılıq istiqaməti: `Api → Persistence, Infrastructure → Application → Domain`.

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
  "Seed": { "TenantCode": "DEMO", "AdminEmail": "admin@demo.az", "AdminPassword": "öz-parolunuz" }
}
```

   `Seed:AdminPassword` təyin olunubsa tətbiq başlayanda ilk universitet və admin yaradılır.

4. API və frontend-i işə salın:

```powershell
dotnet run --project src/Tis.Api
cd frontend; npm install; npm run dev
```

Yeni migration: `dotnet ef migrations add <Ad> -p src/Tis.Persistence -s src/Tis.Api -o Contexts/Migrations`

## Docker ilə

`.env.example` faylını `.env` kimi kopyalayın, `SA_PASSWORD` təyin edin, sonra `docker compose up --build`
və http://localhost:8080 ünvanını açın.

## 1 aylıq plan

| Həftə | Hədəf |
|-------|-------|
| 1 | Tenant, istifadəçi və rollar, JWT giriş, akademik struktur (fakültə, kafedra, ixtisas, qrup) |
| 2 | Fənlər, semestr, dərs cədvəli, müəllim-qrup bağlantısı |
| 3 | Davamiyyət, qiymətlər (50+50 balla, qəbul həddi ayarlana bilən), tələbə və müəllim panelləri |
| 4 | Transkript PDF, elanlar, testlər, deploy, ekran görüntüləri |

## Qərarlar
- Multi-tenant: hər qeyd `TenantId` daşıyır (`TenantEntity`).
- Çoxdillilik: UI mətnləri JSON tərcümə fayllarındadır, API mesajları `Loc.T("açar")` ilə `Locales/*.json`-dan gəlir.
  Dil `Accept-Language` başlığından seçilir, əsas dil `az`-dır. Yeni dil = yeni JSON faylı.
