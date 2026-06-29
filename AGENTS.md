# Lyn — Agent Instructions

Modulær monolitt-backend (.NET 10) + Blazor WebAssembly web + MAUI-apper (PasswordGenerator), med Calorie-app under utvikling. PostgreSQL i Docker.

## Prosjektstruktur

```
Lyn/
├── Lyn.Backend/                  # .NET 10 API — modulær monolitt (Platform/ + Apps/)
├── Lyn.Shared/                   # Delte typer (Result, enums, request/response-modeller)
├── Lyn.Tests/                    # xUnit-tester (unit + integrasjon via Testcontainers)
├── Lyn.Web/                      # Blazor WebAssembly frontend
├── Lyn.Web.Development/          # Dockerfile + nginx for web-frontend
├── PasswordGenerator/            # MAUI-app (multi-target: android/ios/maccatalyst/windows)
├── PasswordGenerator.Core/       # Delt kjerne for MAUI-appen
├── LynPasswordGenerator.Avalonia/# Avalonia-variant
├── docker-compose.dev.yml        # KUN Postgres for dotnet run-arbeidsflyt
├── docker-compose.yml            # Full stack (web + backend + database)
└── docker-compose.ec2.yml        # Prod (backend + database på EC2)
```

### Backend-lagdeling (Lyn.Backend)

```
Lyn.Backend/
├── Common/            # BaseController, delte byggesteiner
├── Infrastructure/    # Persistence (AppDbContext), Email, Storage (S3), Files, Middleware
├── Platform/          # Delte features på tvers av alle apper: Auth, AppReleases, Support
├── Apps/              # Produkter: PasswordGenerator, Calorie
└── Startup/           # ServiceExtensions, PlatformServiceExtensions, Modules/, MiddlewareExtensions
```

## Bygg og kjør

Lokal utvikling: Postgres i Docker + backend og frontend via `dotnet run` på host (ikke containerisert under utvikling).

```bash
# 1. Dev-database (kjører i bakgrunnen)
docker compose -f docker-compose.dev.yml up -d     # Postgres 18 på localhost:5432

# 2. Backend
cd Lyn.Backend && dotnet run                        # http://localhost:8000 (+ /swagger)

# 3. Frontend (Blazor WASM, egen terminal)
cd Lyn.Web && dotnet run                            # http://localhost:7000

# EF-migrasjoner — to kontekster, --context er ALLTID påkrevd
dotnet ef migrations add <Navn> --context AppDbContext -o Infrastructure/Persistence/Migrations
dotnet ef migrations add <Navn> --context CalorieDbContext -o Apps/Calorie/Persistence/Migrations
dotnet ef database update --context AppDbContext
```

## Tester

```bash
# ALLTID target testprosjektet — IKKE "dotnet test" på solution.
# Solution-bygg trekker inn MAUI-appen (net10.0-android) som krever Android SDK.
dotnet test Lyn.Tests/Lyn.Tests.csproj

# Spesifikk gruppe
dotnet test Lyn.Tests/Lyn.Tests.csproj --filter "FullyQualifiedName~Integrations"
```

Integrasjonstester krever at **Docker kjører** (Testcontainers starter en ekte Postgres-container).

## Kritiske regler

**Modeller:** ALDRI opprett nye domenemodeller eller legg til egenskaper uten eksplisitt bekreftelse fra Magee.

**Database — én DB, to kontekster:** `AppDbContext` (Platform + PasswordGenerator) og `CalorieDbContext` (Calorie) deler samme Postgres-database. Calorie har egen migrasjonshistorikk-tabell `__EFMigrationsHistory_Calorie` slik at kontekstene versjoneres uavhengig. Hold dem frikoblet: ingen FK eller JOIN på tvers — `UserId` i Calorie er en ren `Guid`-verdi, ikke en navigasjonsegenskap.

**DB-port:** 5432 overalt i dev (host og container).

**Tester:** Kjør alltid `dotnet test Lyn.Tests/Lyn.Tests.csproj`, aldri `dotnet test` på solution (Android-bygg feiler uten SDK 36).

**Migrasjoner ved oppstart:** Program.cs migrerer begge kontekstene. Legg til nye kontekster der hvis flere produkter kommer til.

## Arkitektur — modulær monolitt

- **`Platform/`** — features delt av alle produkter (Auth, AppReleases, Support).
- **`Apps/[Produkt]/`** — ett produkt per mappe (PasswordGenerator, Calorie), eier egne controllere/services/repositories/DTOs. Hvert produkt registreres via en egen modul i `Startup/Modules/` (`AddPasswordGenerator()`, `AddCalorieModule()`) slik at det kan løftes ut til egen backend senere.
- **Vertical slice** innen hver feature: Controller + Service + Repository + DTOs.

## Feilhåndtering — Result-pattern + ErrorTypeEnum

Services kaster ikke exceptions for domenefeil — de returnerer `Result` / `Result<T>` (i `Lyn.Shared/Result/`) med en `ErrorTypeEnum`:

```csharp
// Service:
if (release == null)
    return Result<ReleaseResponse>.Failure("Release not found", ErrorTypeEnum.NotFound);

// Controller (arver BaseController):
if (result.IsFailure)
    return HandleFailure(result);   // mapper ErrorTypeEnum → HTTP-status + ProblemDetails
```

`ErrorTypeEnum` (i `Lyn.Shared.Enum`) bruker HTTP-statuskoder som verdier: `BadRequest=400`, `Unauthorized=401`, `Forbidden=403`, `NotFound=404`, `Conflict=409`, `Gone=410`, `Validation=422`, `InternalServerError=500`.

`HandleFailure` i `Common/Controllers/BaseController.cs` gjør mappingen. `GlobalExceptionHandler` (Infrastructure/Middleware) fanger uventede exceptions. **Sett aldri HTTP-statuskode manuelt i controllere** — bruk Result + HandleFailure.

## Konfigurasjon — validerte options

Innstillinger bindes som sterkt typede options med DataAnnotations og valideres ved **oppstart** (fail-fast), ikke ved første bruk:

```csharp
services.AddOptions<JwtSettings>()
    .BindConfiguration(JwtSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

Mønsteret gjelder `JwtSettings`, `DatabaseSettings` osv. Connection string leses via `IOptions<DatabaseSettings>` i `AddDbContext`-lambdaene — ikke inline `GetConnectionString() ?? throw`.

## Kodestil

- **Kommentarer:** Norsk. **Identifikatorer og API-navn:** Engelsk.
- **Fil-organisering:** Én ting per fil (DbContext, DTO, service, interface → egen fil).
- **Delte modeller** brukt av flere lag/prosjekter → `Lyn.Shared/`. Produktspesifikke → under produktets mappe.

## Auth og sikkerhet

- **JWT** via `JwtSettings` (issuer/audience/signeringsnøkkel, kort levetid). Validert ved oppstart i `ConfigureJwtBearerOptions`.
- **ASP.NET Identity** med egen brukerklasse (`AppUser`). Roller (Admin, User) og admin-bruker seedes ved oppstart av `DatabaseSeeder`.
- **AppReleases-opplasting** beskyttes av `ReleaseApiKey` (header `X-Api-Key`), ikke JWT.
- **Hemmeligheter:** `.env` skal være gitignorert. Ekte AWS-/Resend-nøkler hører ikke hjemme i innsjekket kode.

## Eksterne tjenester

- **PostgreSQL** — primær database (Docker i både dev og prod, ikke RDS i dag).
- **S3** (`IAmazonS3`) — fillagring (release-filer, support-vedlegg).
- **Resend** (`IResend`) — e-postutsending.

I integrasjonstester mockes `IAmazonS3` og `IResend`; databasen er en ekte Testcontainers-Postgres.

## Mer detaljert dokumentasjon

- Backend: `Lyn.Backend/CLAUDE.md` og `Lyn.Backend/readme.md`
- Roadmap for Calorie + DB: `ROADMAP-Calorie-og-DB.md`
