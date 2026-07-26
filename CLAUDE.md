# Lyn

@AGENTS.md

## Dokumentasjon

Se @Lyn.Backend/CLAUDE.md for backend-arkitektur og patterns (modulær monolitt, Platform/ + Apps/)
Se @Lyn.Web/CLAUDE.md for Blazor WASM-frontend
Se @Calorie/CLAUDE.md for Calorie MAUI-appen (MVVM, Shell-navigasjon, DI, Calorie.Core)

## Status

- **CI/CD & branching (Fase 1.5) — ferdig:** `development` + beskyttet `main` (PR-gate med `build-and-test`), backend bygges i Actions og pushes som privat image til GHCR, EC2 trekker ferdig image. Se `AGENTS.md` + `Lyn.Backend/CLAUDE.md`.
- **Dev-database i Docker (Fase 1) — ferdig:** `docker-compose.dev.yml` (Postgres 18, port 5432) + `dotnet user-secrets` for hemmeligheter.
- **Integrasjonstester — ferdig:** mot ekte Postgres via Testcontainers (speiler AFBack sitt oppsett).
- **Feilhåndtering (AppErrorCode) — ferdig:** `Result` bruker `AppErrorCode` (ikke `ErrorTypeEnum`), `HandleFailure` → `AppProblemDetails`, frontend speiler via `HttpClientExtensions`. Se `AGENTS.md` + `Lyn.Backend/CLAUDE.md`.
- **Localization (Fase 2) — grunnmur ferdig:** `IStringLocalizer` per feature (`en`/`nb`), culture fra JWT `lang`-claim/Accept-Language, `AppUser.PreferredCulture`. Gjenstår: lokaliserte e-poster.

## Pågående / neste

- **Calorie backend (Fase 3) — modeller ferdige:** domenemodeller + samlet `Init`-migrasjon på plass under `Lyn.Backend/Apps/Calorie/`. Gjenstår: slices (kun Products/EAN + Sync + bilder — lokal-først, ingen CRUD-slices) + `AppRelease.Product`-diskriminator.
- **Calorie MAUI-app (Fase 4A/4B + MVVM) — ferdig:** komplett offline kaloriapp i `Calorie` + `Calorie.Core` (SQLite), migrert til MVVM/DI/Shell-ruter juli 2026. Se `Calorie/CLAUDE.md`. Neste: resten av UI-finpuss (4B-2) inkl. store-DI og VM-enhetstester.
- **Localization-rest:** lokaliserte e-poster (verifisering, support).
- **Managed prod-DB (RDS):** utsatt til rett før publisering.
- Følger `ROADMAP-Calorie-og-DB.md`.
