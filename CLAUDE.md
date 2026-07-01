# Lyn

@AGENTS.md

## Dokumentasjon

Se @Lyn.Backend/CLAUDE.md for backend-arkitektur og patterns (modulær monolitt, Platform/ + Apps/)
Se @Lyn.Web/CLAUDE.md for Blazor WASM-frontend

## Status

- **CI/CD & branching (Fase 1.5) — ferdig:** `development` + beskyttet `main` (PR-gate med `build-and-test`), backend bygges i Actions og pushes som privat image til GHCR, EC2 trekker ferdig image. Se `AGENTS.md` + `Lyn.Backend/CLAUDE.md`.
- **Dev-database i Docker (Fase 1) — ferdig:** `docker-compose.dev.yml` (Postgres 18, port 5432) + `dotnet user-secrets` for hemmeligheter.
- **Integrasjonstester — ferdig:** mot ekte Postgres via Testcontainers (speiler AFBack sitt oppsett).

## Pågående / neste

- **Calorie (Fase 3):** nytt produkt under `Lyn.Backend/Apps/Calorie/` med egen `CalorieDbContext` (samme database, egen migrasjonshistorikk). DbContext + modul på plass; domenemodeller gjenstår (krever OK fra Magee).
- **Fase 2:** managed prod-DB (RDS). **Fase 2.5:** localization (felles plumbing, egne ressurser).
- Følger `ROADMAP-Calorie-og-DB.md`.
