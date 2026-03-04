# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Does

Gulf Info Tracker is a bilingual (EN/AR) Gulf-region news aggregator. It ingests articles from government press agencies and international news outlets via RSS, scores each article's credibility using a two-agent Claude pipeline, and exposes the results through a React frontend.

## Commands

### Run the full stack (Aspire orchestrator)
```bash
dotnet run --project src/GulfInfoTracker.AppHost
```
This starts SQL Server (container), Redis (container), the API, and the Vite dev server. The Aspire dashboard is at `https://localhost:15888`.

### Build
```bash
dotnet build GulfInfoTracker.sln
```

### Run all .NET tests
```bash
dotnet test GulfInfoTracker.sln
```

### Run a single .NET test project
```bash
dotnet test tests/GulfInfoTracker.Plugins.Tests/
dotnet test tests/GulfInfoTracker.Api.Tests/
```

### Run a single .NET test by name
```bash
dotnet test tests/GulfInfoTracker.Plugins.Tests/ --filter "FullyQualifiedName~ParseArticle"
```

### Add an EF Core migration
```bash
dotnet ef migrations add <MigrationName> \
  --project src/GulfInfoTracker.Api \
  --startup-project src/GulfInfoTracker.Api
```

### Frontend (React/Vite)
```bash
cd src/GulfInfoTracker.Web
npm install
npm run dev          # dev server
npm run build        # production build
npm run lint         # ESLint
npm test             # Vitest (run once)
npm run test:ui      # Vitest with browser UI
```

## Architecture

### Request flow

```
Browser → Vite dev server (proxy) → ASP.NET Core API → SQL Server
                                                      → Redis (cache)
                                                      → Claude API (scoring/translation)
```

### Backend project map

| Project | Role |
|---|---|
| `GulfInfoTracker.AppHost` | .NET Aspire orchestrator — declares SQL, Redis, API, and Vite resources |
| `GulfInfoTracker.ServiceDefaults` | Shared OpenTelemetry, health checks, and service discovery wiring |
| `GulfInfoTracker.Api` | ASP.NET Core Web API: controllers, EF Core, background services, AI pipeline |
| `GulfInfoTracker.Plugins` | `ISourcePlugin` implementations; no dependency on the API |

### Plugin system (`GulfInfoTracker.Plugins`)

Plugins are loaded from `sources.json` (copied to output dir by the csproj) via `PluginFactory`. Each entry in `sources.json` maps to a concrete `ISourcePlugin` via a `switch` expression in `PluginFactory.cs`.

Inheritance hierarchy:
- `ISourcePlugin` (interface)
  - `GovRssPlugin` — government RSS feeds
  - `RssSourcePlugin` — news outlets (falls back to archive.is for paywalled content)

To add a new source: add an entry to `sources.json`, create a class that extends the appropriate base, and add a `case` in `PluginFactory.cs`. See `PLUGIN_GUIDE.md` for the full walkthrough.

### Ingestion pipeline (`GulfInfoTracker.Api/Services`)

`IngestionService` (a `BackgroundService`) spawns one `PeriodicTimer` loop per enabled plugin (default: 15-minute poll interval). Each tick creates a fresh DI scope and calls `IIngestionProcessor.IngestPluginAsync`. Articles are deduplicated by `SourceUrl` (unique index).

### AI pipeline (`GulfInfoTracker.Api/AI`)

`ClaudeCredibilityPipeline` is a two-step sequential pipeline:

1. **ExtractionAgent** (`claude-opus-4-6`) — extracts named entities, factual claims, and internal consistency rating
2. **ScoringAgent** (`claude-opus-4-6`) — produces a 0–100 credibility score, reasoning text, and 1–3 topic IDs (T1–T5)

Both agents respond with structured JSON only. Topic IDs are assigned inside the ScoringAgent (no third Claude call).

`ClaudeTranslationAgent` (`claude-haiku-4-5-20251001`) handles AR↔EN translation.

The Claude API key is read from `Claude:ApiKey` config or `ANTHROPIC_API_KEY` env var. If absent, the AI services are not registered and scoring/translation background services are skipped.

**Note:** `ScoringBackgroundService` and `TranslationBackgroundService` are registered but currently commented out in `Program.cs`.

### Data model (`GulfInfoTracker.Api/Data`)

EF Core with SQL Server. Key entities:
- `Article` — headline (EN+AR), summary (EN+AR), credibility score, named entities (JSON column), country, pluginId, source URL
- `Topic` — seeded T1–T5 with EN/AR labels
- `ArticleTopic` — many-to-many join
- `SourcePollLog` — per-plugin poll history for health monitoring

Migrations are applied automatically on API startup via `db.Database.MigrateAsync()`.

### API endpoints

- `GET /api/articles` — paged list with `topic`, `country`, `q`, `page`, `pageSize` filters; Redis-cached
- `GET /api/articles/{id}` — article detail
- `GET /api/sources` — source health (last poll time, articles last 24h, last error)
- `POST /api/sources/{id}/poll` — manually trigger a plugin poll

### Frontend (`GulfInfoTracker.Web`)

React 19 + Vite + TypeScript + Tailwind CSS. Key libraries: TanStack Query (data fetching), react-i18next (EN/AR i18n), react-router-dom (routing).

`src/lib/apiClient.ts` — all API calls go through this typed client. The `VITE_API_URL` env var sets the base URL (empty in dev, uses Vite proxy).

`src/hooks/` — React Query hooks (`useArticles`, `useArticle`, `useSources`).

`src/i18n/` — translation strings for EN and AR; RTL layout is handled via Tailwind's `dir` attribute.

## Configuration

### Claude API key (local dev)
Set in `src/GulfInfoTracker.Api/appsettings.Development.json`:
```json
{
  "Claude": { "ApiKey": "sk-ant-..." }
}
```
Or via environment variable: `ANTHROPIC_API_KEY`.

### Enable/disable news sources
Edit `sources.json` at the repo root and set `"enabled": true/false`. Changes take effect on next API restart.
