# Lyn.Backend

.NET 10 API — modulær monolitt. PostgreSQL (Docker). `Platform/` (delt) + `Apps/` (produkter).

## Regler for dette området

Ved arbeid i Lyn.Backend skal disse reglene alltid lastes:

@.claude/rules/architecture.md
@.claude/rules/testing.md

Se også `readme.md` for prod-oppsett (Nginx, Docker, EC2).

## Modulær monolitt

```
Lyn.Backend/
├── Common/            # BaseController (HandleFailure), delte byggesteiner
├── Infrastructure/    # Persistence (AppDbContext), Email (Resend), Storage (S3), Files, Middleware
├── Platform/          # Delte features: Auth, AppReleases, Support
├── Apps/              # Produkter: PasswordGenerator, Calorie
└── Startup/           # ServiceExtensions, PlatformServiceExtensions, Modules/, MiddlewareExtensions
```

- **Platform** = funksjonalitet alle produkter deler. **Apps** = ett produkt per mappe.
- Hvert produkt registreres via en egen modul i `Startup/Modules/` slik at det kan løftes ut til egen backend senere uten å røre sentral wiring.

## Database — én DB, to kontekster

| Kontekst | Eier | Migrasjoner | Historikk-tabell |
|----------|------|-------------|------------------|
| `AppDbContext` | Platform + PasswordGenerator (Identity, AppReleases, Support, statistikk) | `Infrastructure/Persistence/Migrations` | `__EFMigrationsHistory` |
| `CalorieDbContext` | Calorie | `Apps/Calorie/Persistence/Migrations` | `__EFMigrationsHistory_Calorie` |

Begge deler **samme** Postgres-database (`DefaultConnection`). Det eneste som skiller dem er historikk-tabellen, satt via `MigrationsHistoryTable(...)` i `CalorieModule`. Hold kontekstene frikoblet: **ingen FK eller JOIN på tvers** — `UserId` i Calorie er en ren `Guid`-verdi.

```bash
# --context er ALLTID påkrevd siden det er to kontekster
dotnet ef migrations add <Navn> --context CalorieDbContext -o Apps/Calorie/Persistence/Migrations
dotnet ef database update --context AppDbContext
```

`Program.cs` migrerer begge kontekstene ved oppstart. Nye kontekster må legges til der.

## Modul-registrering

```csharp
// Startup/Modules/CalorieModule.cs — eier produktets DbContext + services + repositories
public static IServiceCollection AddCalorieModule(this IServiceCollection services)
{
    services.AddDbContext<CalorieDbContext>((sp, options) =>
    {
        var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        options.UseNpgsql(settings.DefaultConnection, npgsql =>
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Calorie"));
    });
    return services;
}
```

Kalles fra `ConfigureServices` ved siden av `AddPlatform()` / `AddPasswordGenerator()`.

## Auth-system

- **JWT (HS256)** via `JwtSettings` (issuer, audience, symmetrisk signeringsnøkkel, kort levetid). Konfigureres i `Platform/Auth/Options/ConfigureJwtBearerOptions` og valideres ved oppstart.
- **ASP.NET Identity** med egen brukerklasse `AppUser`. Roller (Admin, User) + admin-bruker seedes ved oppstart av `DatabaseSeeder` (`AdminUser:Email`/`AdminUser:Password`).
- **AppReleases-opplasting** beskyttes av `ReleaseApiKey` (header `X-Api-Key`), ikke JWT.

## Konfigurasjon — validerte options (fail-fast)

Innstillinger bindes som sterkt typede options med DataAnnotations og valideres ved **oppstart**, ikke ved første bruk:

```csharp
services.AddOptions<DatabaseSettings>()
    .BindConfiguration(DatabaseSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

Gjelder `JwtSettings`, `DatabaseSettings` osv. Connection string leses via `IOptions<DatabaseSettings>` i `AddDbContext`-lambdaene — aldri inline `GetConnectionString() ?? throw`.

## Gotchas

- **To kontekster:** `--context` er obligatorisk i alle `dotnet ef`-kommandoer.
- **Migrasjons-ERR ved første oppstart:** EF logger en `[ERR]` når den prober en ikke-eksisterende historikk-tabell på fersk DB. Ufarlig — migrasjonene kjøres rett etterpå. Calorie-proben gjentas til første Calorie-migrasjon finnes.
- **Tester:** Kjør `dotnet test Lyn.Tests/Lyn.Tests.csproj`, aldri `dotnet test` på solution (Android-bygg feiler uten SDK 36).
- **Statuskoder:** Sett aldri HTTP-status manuelt i controllere — returner `Result`/`Result<T>` og kall `HandleFailure`.
- **Hemmeligheter:** `.env` skal være gitignorert. Ekte AWS-/Resend-nøkler hører ikke hjemme i innsjekket kode.
