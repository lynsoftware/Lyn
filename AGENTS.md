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

## Branching og CI/CD

**Branch-strategi:**
- `development` — daglig arbeid, fri push.
- `main` — staging/prod. **Beskyttet** (ruleset `ProtectMainRules`): PR påkrevd, status check `build-and-test` må være grønn, ingen force-push, ingen sletting.

Arbeidsflyt: jobb på `development` → push → PR mot `main` → `build-and-test` grønn → merge.

**Workflows** (`.github/workflows/`):
- `test.yml` — `build-and-test` (Testcontainers) på PR mot `main` + push til `development`. Den påkrevde gaten.
- `deploy-backend.yml` — på push til `main`: `test` → `build-and-push` (bygg + push backend-image til GHCR, privat) → `deploy` (EC2 via SSM trekker imaget). Også `workflow_dispatch`.
- `deploy.yml` — Blazor WASM → S3 + CloudFront.

**Deploy-arkitektur:** backend = container på EC2 (image fra GHCR), frontend = statisk S3/CloudFront (ikke container), DB = Postgres i Docker på EC2. Se `Lyn.Backend/CLAUDE.md` for detaljer (GHCR-login, deploy-mekanikk, gotchas).

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

## Feilhåndtering — Result-pattern + AppErrorCode

Services kaster ikke exceptions for domenefeil — de returnerer `Result` / `Result<T>` (i `Lyn.Shared/Result/`) med en `AppErrorCode`. Koden er en **påkrevd** parameter på `Failure`:

```csharp
// Service — ny feil: velg riktig kode
if (release == null)
    return Result<ReleaseResponse>.Failure("Release not found", AppErrorCode.NotFound);

// Service — videresend en nested feil: ta med den indre koden
if (uploadResult.IsFailure)
    return Result.Failure(uploadResult.Error, uploadResult.ErrorCode);

// Controller (arver BaseController):
if (result.IsFailure)
    return HandleFailure(result);   // mapper AppErrorCode → HTTP-status + AppProblemDetails
```

`AppErrorCode` (i `Lyn.Shared/Enum/AppErrorCode.cs`) er domenekontrakten delt mellom backend og frontend, sendt som `code`-felt (int) i responsen. Kode-ranges: `1xxx` generelle, `2xxx` auth, `3xxx` registrering, `4xxx` verifisering, `5xxx` passord-reset, `6xxx` kryptografi.

`HandleFailure` i `Common/Controllers/BaseController.cs` mapper koden til HTTP-statuskode + tittel og returnerer `AppProblemDetails` (`ProblemDetails` + `int Code`). `GlobalExceptionHandler` fanger uventede exceptions. **Sett aldri HTTP-statuskode manuelt i controllere** — bruk Result + HandleFailure.

**Frontend (Lyn.Web)** speiler kontrakten: `HttpClientExtensions.ParseResponseAsync<T>` / `ParseEmptyResponseAsync` (i `Lyn.Web/Common/Extensions/`) leser `detail` + `code` fra svaret og bygger `Result<T>` / `Result` med riktig `AppErrorCode`. ASP.NET model-validation (`errors`-dict) → `AppErrorCode.Validation`. Lokale-/nettverksfeil → `AppErrorCode.Unknown`.

## Localization

Felles plumbing, egne ressurser per feature. `IStringLocalizer<XxxResources>` injiseres i tjenesten som produserer feilmeldingen; `XxxResources` er en `public` markørklasse med `XxxResources.resx` (+ `.nb.resx`) i samme mappe/namespace. Backend oversetter ved kilden, så `Result`/`HandleFailure` er uendret.

- **Oppsett:** `AddAppLocalization()` (i `Startup/LocalizationExtensions.cs`) + `app.UseRequestLocalization()` (etter `UseAuthentication`).
- **Culture-resolving:** JWT `lang`-claim (autentiserte) → `Accept-Language` (anonyme) → default `en`. Støttede kulturer i `SupportedCultures` (`Platform/Localization/`).
- **Brukerspråk:** `AppUser.PreferredCulture` (BCP-47) stemples inn som `lang`-claim ved login.

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
