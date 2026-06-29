# Arkitekturregler — Lyn.Backend

## Modulær monolitt: Platform vs Apps

```
Platform/[Feature]/     # Delt på tvers av alle produkter (Auth, AppReleases, Support)
  ├── Controllers/
  ├── Services/
  ├── Repositories/
  ├── Models/
  └── DTOs/

Apps/[Produkt]/         # Ett produkt (PasswordGenerator, Calorie)
  ├── Controllers/
  ├── Services/
  ├── Repositories/
  ├── Persistence/      # Kun for produkter med egen DbContext (f.eks. Calorie)
  └── DTOs/
```

- **Platform** eier delt funksjonalitet. **Apps** eier ett produkt hver.
- Vertical slice innen hver feature: Controller + Service + Repository + DTOs.
- Et produkt skal kunne løftes ut til egen backend — unngå at Apps lekker inn i Platform (ingen FK/JOIN på tvers av produkt-grenser).

## Modul-konvensjon

Hvert produkt/område har en `Add...`-extension som samler all DI på ett sted:

```csharp
// Startup/Modules/PasswordGeneratorModule.cs
public static IServiceCollection AddPasswordGenerator(this IServiceCollection services) { ... }

// Startup/PlatformServiceExtensions.cs
public static IServiceCollection AddPlatform(this IServiceCollection services)
{
    services.AddAppReleases();
    services.AddSupport();
    services.AddAuth();
    return services;
}
```

Kalles fra `ServiceExtensions.ConfigureServices`. Et produkt med egen DbContext registrerer den i sin egen modul (se `CalorieModule`).

## DTO-navnekonvensjon

- **`Request`** — data fra klient til backend
- **`Response`** — data fra backend til klient
- **`Dto`** — intern bruk / mapping mellom lag
- Delte modeller brukt av flere prosjekter → `Lyn.Shared/`. Produktspesifikke → under produktets mappe.

## Result-pattern + ErrorTypeEnum

Services kaster ikke exceptions for domenefeil — de returnerer `Result` / `Result<T>` (`Lyn.Shared/Result/`):

```csharp
// Service:
if (user == null)
    return Result<UserResponse>.Failure("User not found", ErrorTypeEnum.NotFound);

// Controller (arver BaseController):
if (result.IsFailure)
    return HandleFailure(result);   // ErrorTypeEnum → HTTP-status + ProblemDetails
```

`ErrorTypeEnum` (`Lyn.Shared.Enum`) bruker HTTP-koder som verdier:

```
BadRequest = 400, Unauthorized = 401, Forbidden = 403, NotFound = 404,
Conflict = 409, Gone = 410, Validation = 422, InternalServerError = 500
```

`HandleFailure` i `Common/Controllers/BaseController.cs` gjør mappingen til `ProblemDetails`. `GlobalExceptionHandler` (`Infrastructure/Middleware`) fanger uventede exceptions. **Sett aldri statuskode manuelt** i controllere.

## Konfigurasjon — validerte options

Sterkt typede options med DataAnnotations, validert ved oppstart:

```csharp
// Options-klasse med DataAnnotations
public class DatabaseSettings
{
    public const string SectionName = "ConnectionStrings";

    [Required] public string DefaultConnection { get; set; } = string.Empty;
}

// Registrering — fail-fast ved oppstart
services.AddOptions<DatabaseSettings>()
    .BindConfiguration(DatabaseSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

Framework-options (f.eks. `JwtBearerOptions`) konfigureres via en egen `IConfigureOptions`-klasse (`ConfigureJwtBearerOptions`). DbContext konfigureres via `AddDbContext((sp, options) => ...)` som leser `IOptions<DatabaseSettings>` — ikke inline `GetConnectionString() ?? throw`.

## Persistens — to kontekster, én database

- `AppDbContext` (Platform + PasswordGenerator) og `CalorieDbContext` (Calorie) deler samme Postgres-DB.
- Calorie har egen historikk-tabell `__EFMigrationsHistory_Calorie` (`MigrationsHistoryTable(...)`).
- `--context` er påkrevd i alle `dotnet ef`-kommandoer.
- Begge migreres ved oppstart i `Program.cs`.
