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

## Result-pattern + AppErrorCode (delt med backend)

Services bruker samme `Lyn.Shared.Result`-type som backend og returnerer `Result` / `Result<T>` med en `AppErrorCode` (påkrevd). Parsing av backend-svar går gjennom `HttpClientExtensions` (`Common/Extensions/`) — ikke gjenta ProblemDetails-lesing i hver tjeneste:

```csharp
public async Task<Result<PasswordGenerationResponse>> GeneratePasswordAsync(...)
{
    try
    {
        var response = await httpClient.PostAsJsonAsync("api/passwordgenerator", request, ct);
        // Leser code + detail ved feil, deserialiserer body ved suksess
        return await HttpClientExtensions.ParseResponseAsync<PasswordGenerationResponse>(response, ct);
    }
    catch (HttpRequestException ex) { return Result<...>.Failure("...", AppErrorCode.Unknown); }
}
```

- **`ParseResponseAsync<T>`** — endepunkter med verdi. **`ParseEmptyResponseAsync`** — void/204-endepunkter.
- Backend-koden flyter helt ut: leser `code` (int → `AppErrorCode`) + `detail` fra `AppProblemDetails`. ASP.NET model-validation (`errors`-dict) → `AppErrorCode.Validation`.
- **Lokale-/nettverksfeil** (i `catch`, tom token, binær nedlasting): sett `AppErrorCode.Unknown` eksplisitt.
- Binære svar (filnedlasting) kan ikke bruke `ParseResponseAsync` på suksess-stien — les bytes manuelt, og bruk `ParseEmptyResponseAsync` kun i feil-grenen for å hente koden.

## Gotchas

- **Token-lagring:** admin-token lagres i **SessionStorage** (nøkkel `adminToken`), ikke LocalStorage.
- **Delte modeller:** request/response-typer ligger i `Lyn.Shared/Models/` — ikke dupliser dem i `DTOs/`.
- **Streng-responser:** token returneres som JSON-streng — `ParseResponseAsync<string>` fjerner anførselstegn selv (`ReadFromJsonAsync`), så ikke `Trim('"')` manuelt.
- **Globalisering:** bruk `Lyn.Web/Resources` + `LocalizationService` — ikke hardkodede strenger.
