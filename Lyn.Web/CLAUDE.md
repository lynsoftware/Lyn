# Lyn.Web

Blazor WebAssembly-frontend for Lyn (admin/web for PasswordGenerator: nedlasting av releaser, support, admin-innlogging).

## Tech stack

- **Blazor WebAssembly** (.NET 10) — standalone WASM, kjører i nettleser
- **Blazor.Bootstrap** — UI-komponenter
- **Blazored.LocalStorage / SessionStorage** — klientlagring
- **Microsoft.Extensions.Localization** — globalisering
- **Serilog** (BrowserConsole-sink) — logging i nettleserkonsollen

## Kjør

```bash
cd Lyn.Web && dotnet run        # http://localhost:7000
```

Krever at backend kjører på `http://localhost:8000` (se rot-`AGENTS.md` for full host-arbeidsflyt). Backend må ha `http://localhost:7000` i `Cors:AllowedOrigins`.

## Struktur

```
Lyn.Web/
├── Pages/         # Rutede sider (.razor)
├── Components/     # Gjenbrukbare komponenter
├── Layout/         # Layout-komponenter
├── Services/
│   ├── Api/        # Backend-kall: AuthService, DownloadService, PasswordGenerationService, SupportTicketService
│   ├── ThemeService.cs
│   └── LocalizationService.cs
├── DTOs/           # Frontend-spesifikke DTOer (delte ligger i Lyn.Shared)
└── wwwroot/        # appsettings.json (ApiBaseUrl), statiske filer
```

## API-konfigurasjon

Backend-URL leses fra config (`BACKEND_URL` eller `ApiBaseUrl`), satt i `wwwroot/appsettings.json`:

```json
{ "ApiBaseUrl": "http://localhost:8000" }
```

`HttpClient` registreres i `Program.cs` med denne som `BaseAddress`. Mangler den, kaster appen ved oppstart.

## Result-pattern (delt med backend)

Services bruker samme `Lyn.Shared.Result`-type som backend — de returnerer `Result` / `Result<T>` i stedet for å kaste:

```csharp
public async Task<Result> LoginAsync(LoginRequest request, CancellationToken ct)
{
    var response = await httpClient.PostAsJsonAsync("api/admin/login", request, ct);
    if (!response.IsSuccessStatusCode)
        return Result.Failure(await response.Content.ReadAsStringAsync(ct));
    // ...
    return Result.Success();
}
```

## Gotchas

- **Token-lagring:** admin-token lagres i **SessionStorage** (nøkkel `adminToken`), ikke LocalStorage.
- **Delte modeller:** request/response-typer ligger i `Lyn.Shared/Models/` — ikke dupliser dem i `DTOs/`.
- **Void/streng-responser:** flere backend-endepunkter returnerer rå streng (f.eks. token) — `ReadAsStringAsync` + `Trim('"')`, ikke alltid JSON.
- **Globalisering:** bruk `Lyn.Web/Resources` + `LocalizationService` — ikke hardkodede strenger.
