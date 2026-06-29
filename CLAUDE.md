# Lyn

@AGENTS.md

## Dokumentasjon

Se @Lyn.Backend/CLAUDE.md for backend-arkitektur og patterns (modulær monolitt, Platform/ + Apps/)
Se @Lyn.Web/CLAUDE.md for Blazor WASM-frontend

## Pågående arbeid

- **Calorie:** nytt produkt under `Lyn.Backend/Apps/Calorie/` med egen `CalorieDbContext` (samme database, egen migrasjonshistorikk). Følger `ROADMAP-Calorie-og-DB.md` (Fase 1-6). MAUI-app + backend-modul.
- **Database i Docker:** dev-DB via `docker-compose.dev.yml`. Prod kjører Postgres i Docker på EC2 (ikke RDS i dag) — RDS er planlagt (Fase 2).
- **Integrasjonstester:** kjører mot ekte Postgres via Testcontainers (speiler AFBack sitt oppsett).
