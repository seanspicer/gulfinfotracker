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

### Configuration reference

All secrets can be set either in `src/GulfInfoTracker.Api/appsettings.Development.json` or as environment variables. Environment variables use `__` as a section separator (e.g. `Api__BearerToken`).

| Config key | Env var | Description |
|---|---|---|
| `Api:BearerToken` | `Api__BearerToken` | Bearer token required on all `/api/*` requests. Leave empty to skip auth (dev only). |
| `AiProvider` | `AiProvider` | `"OpenAi"` (default) or `"Claude"` |
| `OpenAi:ApiKey` | `OPENAI_API_KEY` | OpenAI key for GPT-based credibility scoring |
| `Claude:ApiKey` | `ANTHROPIC_API_KEY` | Anthropic key for Claude-based scoring |
| `X:BearerToken` | `X_BEARER_TOKEN` | X (Twitter) API v2 key — only needed if an X source plugin is enabled |

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

### Authentication

Every `/api/*` request must include a bearer token in the `Authorization` header:

```
Authorization: Bearer <your-token>
```

Configure the expected token in `src/GulfInfoTracker.Api/appsettings.Development.json`:

```json
{
  "Api": { "BearerToken": "my-secret-token" }
}
```

Or set the environment variable `Api__BearerToken=my-secret-token`.

The frontend reads the same token from `src/GulfInfoTracker.Web/.env.local`:

```
VITE_API_BEARER_TOKEN=my-secret-token
```

| Scenario | Result |
|---|---|
| Correct token | Request proceeds normally |
| Wrong or missing token | `401 Unauthorized` + `WWW-Authenticate: Bearer` header |
| Token not configured (empty) | Warning logged; all requests pass through (dev convenience) |

Health check endpoints (`/healthz`, etc.) and the OpenAPI spec (`/openapi/v1.json`) are **not** protected.

---

### `GET /api/articles`

Returns a paged list of **scored** articles. Results are Redis-cached. Unscored articles are held back until the AI pipeline processes them.

**Query parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `topic` | string | — | Filter by topic ID: `T1` – `T5` |
| `country` | string | — | Filter by country code: `UAE`, `SA`, `QA`, `BH`, `KW`, `OM`, `INTL` |
| `q` | string | — | Full-text search across EN headline and summary |
| `sortBy` | string | `newest` | `newest`, `oldest`, or `score` |
| `page` | int | `1` | Page number (min: 1) |
| `pageSize` | int | `20` | Items per page (max: 100) |
| `sources` | string[] | — | Repeated param — filter to specific plugin IDs. Can be specified multiple times. |

**Example request:**

```bash
curl "https://localhost:5001/api/articles?country=UAE&topic=T2&sortBy=score&sources=gulf-news&sources=the-national" \
  -H "Authorization: Bearer my-secret-token"
```

**Response `200 OK`:**

```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "pluginId": "gulf-news",
      "headlineEn": "UAE announces new economic zone",
      "headlineAr": "الإمارات تعلن عن منطقة اقتصادية جديدة",
      "summaryEn": "The UAE government announced plans for a new free-trade zone...",
      "summaryAr": null,
      "sourceUrl": "https://gulfnews.com/uae/economy/uae-announces-new-economic-zone-1.12345",
      "publishedAt": "2026-01-15T09:30:00Z",
      "country": "UAE",
      "credibilityScore": 82,
      "fullText": true,
      "translated": false,
      "topicIds": ["T2", "T4"]
    }
  ],
  "total": 347,
  "page": 1,
  "pageSize": 20
}
```

| Field | Type | Description |
|---|---|---|
| `data` | array | Articles for the current page |
| `total` | int | Total matching articles (across all pages) |
| `page` | int | Current page number |
| `pageSize` | int | Items per page |
| `data[].id` | uuid | Article identifier |
| `data[].pluginId` | string | Source plugin that ingested this article |
| `data[].headlineEn` | string | English headline |
| `data[].headlineAr` | string\|null | Arabic headline (null until translated) |
| `data[].summaryEn` | string\|null | English summary |
| `data[].summaryAr` | string\|null | Arabic summary (null until translated) |
| `data[].sourceUrl` | string | Canonical URL of the original article |
| `data[].publishedAt` | ISO 8601 | Article publication timestamp |
| `data[].country` | string | Country code (e.g. `UAE`, `INTL`) |
| `data[].credibilityScore` | int\|null | 0–100 score; null if not yet scored |
| `data[].fullText` | bool | Whether full article body was captured |
| `data[].translated` | bool | Whether AR↔EN translation has been applied |
| `data[].topicIds` | string[] | Topic tags assigned by the AI (subset of T1–T5) |

---

### `GET /api/articles/{id}`

Returns full detail for a single article, including the AI's credibility reasoning and ingestion timestamp.

```bash
curl "https://localhost:5001/api/articles/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer my-secret-token"
```

**Response `200 OK`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "pluginId": "gulf-news",
  "headlineEn": "UAE announces new economic zone",
  "headlineAr": "الإمارات تعلن عن منطقة اقتصادية جديدة",
  "summaryEn": "The UAE government announced plans for a new free-trade zone...",
  "summaryAr": null,
  "sourceUrl": "https://gulfnews.com/uae/economy/uae-announces-new-economic-zone-1.12345",
  "publishedAt": "2026-01-15T09:30:00Z",
  "ingestedAt": "2026-01-15T09:45:12Z",
  "country": "UAE",
  "credibilityScore": 82,
  "credibilityReasoning": "The article cites named officials and references a verifiable government announcement. No contradictory claims were detected across corroborating sources.",
  "fullText": true,
  "translated": false,
  "topicIds": ["T2", "T4"]
}
```

All fields from `ArticleListItem` are included, plus:

| Field | Type | Description |
|---|---|---|
| `ingestedAt` | ISO 8601 | When the article was first stored in the database |
| `credibilityReasoning` | string\|null | AI-generated explanation of the credibility score |

**`404 Not Found`** is returned when the ID does not exist.

---

### `GET /api/sources`

Returns a health snapshot for every registered plugin: when it was last polled, how many articles it produced in the past 24 hours, and any recent error.

```bash
curl "https://localhost:5001/api/sources" \
  -H "Authorization: Bearer my-secret-token"
```

**Response `200 OK`:**

```json
[
  {
    "pluginId": "gulf-news",
    "displayName": "Gulf News",
    "lastPolledAt": "2026-01-15T10:00:00Z",
    "articlesLast24h": 14,
    "lastError": null
  },
  {
    "pluginId": "wsj",
    "displayName": "Wall Street Journal",
    "lastPolledAt": "2026-01-15T09:45:00Z",
    "articlesLast24h": 3,
    "lastError": "HTTP 429: rate limited by archive.is"
  }
]
```

| Field | Type | Description |
|---|---|---|
| `pluginId` | string | Unique plugin identifier (matches `sources.json`) |
| `displayName` | string | Human-readable source name |
| `lastPolledAt` | ISO 8601\|null | Last successful poll timestamp; null if never polled |
| `articlesLast24h` | int | Articles ingested from this source in the past 24 hours |
| `lastError` | string\|null | Most recent error message; null if last poll succeeded |

---

### `POST /api/sources/{id}/poll`

Manually triggers an immediate ingest cycle for the named plugin. Useful during development or after enabling a new source. The poll runs asynchronously — the response is returned before ingestion completes.

```bash
curl -X POST "https://localhost:5001/api/sources/gulf-news/poll" \
  -H "Authorization: Bearer my-secret-token"
```

**Response `200 OK`:**

```json
{ "message": "Poll triggered for 'gulf-news'." }
```

**`404 Not Found`** is returned when no plugin with that ID is registered.

---

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
