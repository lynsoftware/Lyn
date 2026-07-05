# Lyn

@AGENTS.md

## Dokumentasjon

Se @Lyn.Backend/CLAUDE.md for backend-arkitektur og patterns (modulær monolitt, Platform/ + Apps/)
Se @Lyn.Web/CLAUDE.md for Blazor WASM-frontend

## Status

- **CI/CD & branching (Fase 1.5) — ferdig:** `development` + beskyttet `main` (PR-gate med `build-and-test`), backend bygges i Actions og pushes som privat image til GHCR, EC2 trekker ferdig image. Se `AGENTS.md` + `Lyn.Backend/CLAUDE.md`.
- **Dev-database i Docker (Fase 1) — ferdig:** `docker-compose.dev.yml` (Postgres 18, port 5432) + `dotnet user-secrets` for hemmeligheter.
- **Integrasjonstester — ferdig:** mot ekte Postgres via Testcontainers (speiler AFBack sitt oppsett).
- **Feilhåndtering (AppErrorCode) — ferdig:** `Result` bruker `AppErrorCode` (ikke `ErrorTypeEnum`), `HandleFailure` → `AppProblemDetails`, frontend speiler via `HttpClientExtensions`. Se `AGENTS.md` + `Lyn.Backend/CLAUDE.md`.
- **Localization (Fase 2) — grunnmur ferdig:** `IStringLocalizer` per feature (`en`/`nb`), culture fra JWT `lang`-claim/Accept-Language, `AppUser.PreferredCulture`. Gjenstår: lokaliserte e-poster.

## Pågående / neste

- **Calorie (Fase 3):** nytt produkt under `Lyn.Backend/Apps/Calorie/` med egen `CalorieDbContext` (samme database, egen migrasjonshistorikk). DbContext + modul på plass; domenemodeller gjenstår (krever OK fra Magee).
- **Localization-rest:** lokaliserte e-poster (verifisering, support).
- **Managed prod-DB (RDS):** utsatt til rett før publisering.
- Følger `ROADMAP-Calorie-og-DB.md`.
