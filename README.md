# Gulf Info Tracker

A bilingual (English/Arabic) news aggregator for the Gulf region. Articles are ingested from government press agencies and international news outlets via RSS, scored for credibility by a two-agent AI pipeline, and served through a React frontend.

---

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 10.0+ |
| Docker Desktop | Running (Aspire launches PostgreSQL and Redis containers) |
| Node.js | 20+ (LTS) |
| OpenAI API key | Optional — scoring is skipped when absent (Claude is also supported) |

---

## Running with .NET Aspire

Aspire orchestrates every service in a single command. It starts PostgreSQL and Redis as containers, runs the ASP.NET Core API, and launches the Vite dev server.

```bash
dotnet run --project src/GulfInfoTracker.AppHost
```

Once running, open the **Aspire dashboard** at `https://localhost:15888` to view logs, traces, and health checks for every resource.

> **Note:** On first run, Docker will pull the PostgreSQL and Redis images. This may take a minute.
> The PostgreSQL data is stored in a named Docker volume (`gulf-info-tracker-pgdata`) and **persists across restarts**. To wipe the database: `docker volume rm gulf-info-tracker-pgdata`.

### Setting the AI API key (local dev)

The default AI provider is **OpenAI**. Set the key in `src/GulfInfoTracker.Api/appsettings.Development.json`:

```json
{
  "OpenAi": {
    "ApiKey": "sk-..."
  }
}
```

Alternatively, export the environment variable before running:

```bash
export OPENAI_API_KEY=sk-...
dotnet run --project src/GulfInfoTracker.AppHost
```

To use **Claude** instead, set `AiProvider` to `"Claude"` and supply the Anthropic key:

```json
{
  "AiProvider": "Claude",
  "Claude": {
    "ApiKey": "sk-ant-..."
  }
}
```

Or via environment variable: `ANTHROPIC_API_KEY`.

Without any API key the application still runs — articles are ingested and stored, but credibility scoring is disabled.

---

## API Reference

The API is served by the ASP.NET Core project (`GulfInfoTracker.Api`). All endpoints are under `/api/`.

### `GET /api/articles`

Returns a paged list of scored articles. Results are Redis-cached.

**Query parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `topic` | string | — | Filter by topic ID (`T1`–`T5`) |
| `country` | string | — | Filter by ISO country code (`UAE`, `SA`, `QA`, `BH`, `KW`, `OM`, `INTL`) |
| `q` | string | — | Full-text search across headline and summary |
| `sortBy` | string | `newest` | Sort order: `newest`, `oldest`, or `score` |
| `page` | int | `1` | Page number |
| `pageSize` | int | `20` | Articles per page (max: `100`) |

> Only articles with a credibility score are returned. Unscored articles are held back until the AI pipeline processes them.

**Example:**

```
GET /api/articles?country=UAE&topic=T2&sortBy=score&page=1&pageSize=10
```

### `GET /api/articles/{id}`

Returns a single article by ID, including named entities, topic tags, credibility score, and reasoning.

### `GET /api/sources`

Returns health information for every registered plugin: last poll time, number of articles ingested in the last 24 hours, and the last error message (if any).

### `POST /api/sources/{id}/poll`

Manually triggers an immediate poll for the given plugin ID (e.g. `gulf-news`, `qatar-qna`). Useful during development or to force a refresh after enabling a new source.

```bash
curl -X POST https://localhost:5001/api/sources/gulf-news/poll
```

### OpenAPI / Swagger

In development mode the OpenAPI spec is available at:

```
https://localhost:5001/openapi/v1.json
```

---

## Sources

Sources are declared in **`sources.json`** at the repository root. Each entry maps to a plugin class that handles fetching and parsing for that outlet.

### Registered sources

| Plugin ID | Display Name | Country | Type | Default Interval |
|---|---|---|---|---|
| `gulf-news` | Gulf News | UAE | news | 30 min |
| `khaleej-times` | Khaleej Times | UAE | news | 30 min |
| `the-national` | The National | UAE | news | 30 min |
| `arabian-business` | Arabian Business | UAE | news | 30 min |
| `uae-ncema` | UAE National Media Center | UAE | government | 15 min |
| `uae-mod` | UAE Ministry of Defence | UAE | government | 60 min |
| `qatar-qna` | Qatar News Agency | QA | government | 15 min |
| `qatar-qna-economy` | Qatar News Agency — Economy | QA | government | 15 min |
| `al-jazeera` | Al Jazeera English | QA | news | 30 min |
| `kuwait-times` | Kuwait Times | KW | news | 30 min |
| `saudi-gazette` | Saudi Gazette | SA | news | 30 min |
| `saudi-gazette-economy` | Saudi Gazette — Economy | SA | news | 30 min |
| `oman-ona` | Oman News Agency | OM | government | 15 min |
| `oman-ona-economy` | Oman News Agency — Economy | OM | government | 15 min |
| `times-of-oman` | Times of Oman | OM | news | 30 min |
| `muscat-daily` | Muscat Daily | OM | news | 30 min |
| `bahrain-bna` | Bahrain News Agency | BH | government | 15 min |
| `bahrain-bna-business` | Bahrain News Agency — Business | BH | government | 15 min |
| `bloomberg-markets` | Bloomberg Markets | INTL | news | 30 min |
| `bloomberg-economics` | Bloomberg Economics | INTL | news | 30 min |
| `wsj` | Wall Street Journal | INTL | news | 30 min |
| `nyt` | New York Times (Middle East) | INTL | news | 30 min |

### `sources.json` fields

```jsonc
{
  "pluginId": "gulf-news",       // Unique ID — used in API routes and logs
  "displayName": "Gulf News",    // Human-readable name shown in the UI
  "country": "UAE",              // ISO code or INTL
  "type": "government|news",     // Selects the plugin base class
  "enabled": true,               // false = skipped on startup, no polling
  "pollIntervalMinutes": 30,     // How often to check for new articles
  "feedUrl": "https://..."       // RSS/Atom feed URL
}
```

### Enabling / disabling a source

Set `"enabled": false` to stop polling a source without removing its entry. The change takes effect on the next API restart.

### Adding a new source

See **[PLUGIN_GUIDE.md](PLUGIN_GUIDE.md)** for a full walkthrough. The short version:

1. Create a class extending `GovRssPlugin` or `RssSourcePlugin` (or implement `ISourcePlugin` directly).
2. Add an entry to `sources.json`.
3. Add a `case` for the new `pluginId` in `src/GulfInfoTracker.Plugins/PluginFactory.cs`.

---

## AI Pipeline

Credibility scoring runs automatically in the background (`ScoringBackgroundService`) once an API key is configured. Two agents run sequentially on each ingested article:

1. **ExtractionAgent** — extracts named entities, factual claims, and an internal consistency rating.
2. **ScoringAgent** — produces a 0–100 credibility score, a reasoning summary, and 1–3 topic tags (T1–T5).

The active provider is controlled by the `AiProvider` config key:

| `AiProvider` | Models used |
|---|---|
| `OpenAi` (default) | `gpt-4.1-nano` for both extraction and scoring |
| `Claude` | `claude-opus-4-6` for both extraction and scoring |

Topic IDs returned by the model are validated against the T1–T5 set before being persisted. Invalid values are silently dropped.

> AR↔EN translation is implemented (`TranslationBackgroundService`) but currently disabled. It can be re-enabled in `Program.cs`.

### Topics

| ID | English | Arabic |
|---|---|---|
| T1 | Politics & Government | السياسة والحكومة |
| T2 | Economy & Finance | الاقتصاد والمالية |
| T3 | Energy & Oil | الطاقة والنفط |
| T4 | Business & Investment | الأعمال والاستثمار |
| T5 | Iran / Israel / US Conflict | الصراع الإيراني / الإسرائيلي / الأمريكي |

---

## Architecture

```
Browser
  └─→ Vite dev server (proxy)
        └─→ ASP.NET Core API
              ├─→ PostgreSQL  (articles, topics, poll logs)
              ├─→ Redis       (API response cache)
              └─→ OpenAI / Claude API  (credibility scoring)
```

### Project map

| Project | Role |
|---|---|
| `GulfInfoTracker.AppHost` | .NET Aspire 13 orchestrator |
| `GulfInfoTracker.ServiceDefaults` | Shared OpenTelemetry, health checks, service discovery |
| `GulfInfoTracker.Api` | ASP.NET Core Web API, EF Core, background services, AI pipeline |
| `GulfInfoTracker.Plugins` | `ISourcePlugin` implementations — no dependency on the API |
| `GulfInfoTracker.Web` | React 19 + Vite + TypeScript + Tailwind CSS frontend |

---

## Other Commands

```bash
# Build the solution
dotnet build GulfInfoTracker.sln

# Run all tests
dotnet test GulfInfoTracker.sln

# Add an EF Core migration
dotnet ef migrations add <MigrationName> \
  --project src/GulfInfoTracker.Api \
  --startup-project src/GulfInfoTracker.Api

# Frontend only
cd src/GulfInfoTracker.Web
npm install
npm run dev        # standalone dev server (no API proxy)
npm test           # Vitest
npm run lint       # ESLint
```
