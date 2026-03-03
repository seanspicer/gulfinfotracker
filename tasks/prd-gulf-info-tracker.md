# PRD: Gulf Info Tracker

## Introduction

Gulf Info Tracker is a mobile-first public web application that aggregates news and official statements
exclusively from verified, authoritative sources covering the Gulf region (UAE, Saudi Arabia, Qatar,
Bahrain) and monitors coverage of the Iran/Israel/US conflict. Each article is automatically evaluated
by an AI agent team (powered by Anthropic Claude) which produces a credibility score and human-readable
reasoning. Sources are implemented as independently deployable plugins so new outlets can be added
without changing core application code.

The entire solution — React frontend, ASP.NET Core API, Redis cache, and SQL database — is
orchestrated locally and deployed to Azure using **.NET Aspire**. Aspire provides the AppHost
project that wires all services together, injects connection strings and service URLs automatically,
and publishes a deployment manifest consumed by the Azure Developer CLI (`azd`) to provision Azure
Container Apps.

The application solves a specific problem: in a region where state-affiliated media and genuine
official government communications co-exist with speculation and disinformation, readers need a
single, trusted aggregator that distinguishes verified official information from editorial opinion —
with transparent AI-assisted reasoning for every scoring decision.

---

## Goals

- Aggregate content only from a defined allowlist of authoritative government portals and tier-1
  news outlets.
- Score every article for credibility (0–100) using a multi-agent Claude pipeline, and surface the
  reasoning alongside the score.
- Display all content bilingually (English + Arabic) with machine translation where the original
  language differs from the reader's preference.
- Allow new sources to be added by implementing a plugin interface and registering configuration —
  zero changes to core ingestion or scoring logic.
- Deliver a fast, usable experience on mobile devices (< 3G-equivalent conditions considered).
- Deploy entirely on Azure with no vendor lock-in at the application layer.
- Use .NET Aspire as the single orchestration layer for local development and Azure deployment,
  eliminating manual Docker Compose or per-service launch configuration.

---

## Topic Categories

| ID  | Label                        |
|-----|------------------------------|
| T1  | Politics & Government        |
| T2  | Economy & Finance            |
| T3  | Energy & Oil                 |
| T4  | Business & Investment        |
| T5  | Iran / Israel / US Conflict  |

---

## Initial Source Allowlist

### Official Government Sources
| Plugin ID          | Country      | Primary Method        | Fallback     |
|--------------------|--------------|-----------------------|--------------|
| `uae-gov`          | UAE          | UAE Government API    | RSS / scrape |
| `saudi-spa`        | Saudi Arabia | SPA (Saudi Press Agency) API | RSS  |
| `qatar-qna`        | Qatar        | QNA official RSS      | Scrape       |
| `bahrain-bna`      | Bahrain      | BNA official RSS      | Scrape       |

### Tier-1 News Outlets
| Plugin ID  | Outlet           | Method                     |
|------------|------------------|----------------------------|
| `ft`       | Financial Times  | RSS feed + archive.is try  |
| `wsj`      | Wall Street Journal | RSS feed + archive.is try|
| `nyt`      | New York Times   | RSS feed + archive.is try  |

---

## User Stories

### US-001: Browse the news feed
**Description:** As a visitor, I want to see a reverse-chronological list of recent verified articles
so that I can quickly scan what is happening across the Gulf region.

**Acceptance Criteria:**
- [ ] Feed displays article cards sorted newest-first.
- [ ] Each card shows: headline, source name + logo, publication timestamp, topic badge, credibility
      score badge, and a 2–3 sentence summary.
- [ ] Feed is paginated (infinite scroll or "load more") — initial load is ≤ 20 items.
- [ ] Feed loads and is interactive within 3 seconds on a throttled 3G connection (Lighthouse mobile).
- [ ] Verify in browser using dev-browser skill.

---

### US-002: Filter feed by topic
**Description:** As a visitor, I want to filter articles by topic (e.g. Energy & Oil only) so that I
can focus on the subject matter I care about.

**Acceptance Criteria:**
- [ ] Topic filter chips/tabs visible at top of feed (T1–T5 + "All").
- [ ] Selecting a topic updates the feed immediately without full page reload.
- [ ] Active filter is reflected in the URL query string (`?topic=energy`).
- [ ] "All" is the default active state on first load.
- [ ] Verify in browser using dev-browser skill.

---

### US-003: Filter feed by country
**Description:** As a visitor, I want to filter articles by country so that I can focus on news from
a specific Gulf state.

**Acceptance Criteria:**
- [ ] Country filter available (UAE, Saudi Arabia, Qatar, Bahrain, "All").
- [ ] Country filter can be combined with topic filter (AND logic).
- [ ] Active country filter reflected in URL (`?country=uae`).
- [ ] Verify in browser using dev-browser skill.

---

### US-004: View article detail with credibility report
**Description:** As a visitor, I want to open an article and see the AI credibility report so that I
understand why it received its score.

**Acceptance Criteria:**
- [ ] Clicking a card opens an article detail view (full-page or bottom sheet on mobile).
- [ ] Detail view shows: full headline, source, date, original article link, full summary, topic,
      country.
- [ ] Credibility score (0–100) displayed prominently with a colour-coded tier label
      (Verified ≥ 80, Credible 60–79, Uncertain 40–59, Low < 40).
- [ ] AI reasoning text displayed below the score (plain prose, not JSON).
- [ ] "View original source" link opens the official source in a new tab.
- [ ] Verify in browser using dev-browser skill.

---

### US-005: Bilingual content (English / Arabic)
**Description:** As an Arabic-speaking visitor, I want to read content in Arabic so that the
application is accessible to the primary audience of the region.

**Acceptance Criteria:**
- [ ] Language toggle (EN / AR) visible in the header on every page.
- [ ] Selecting AR switches the UI to right-to-left (RTL) layout.
- [ ] Article headlines and summaries are displayed in the selected language.
- [ ] If original content is in English and AR is selected, a machine-translated Arabic version is
      shown with a "Machine translated" label.
- [ ] Language preference is persisted in localStorage.
- [ ] Verify in browser using dev-browser skill.

---

### US-006: Source plugin — ingest a government source
**Description:** As a developer, I want a well-defined plugin interface so that I can add a new
government source by implementing a single class and registering config — without changing core code.

**Acceptance Criteria:**
- [ ] A `ISourcePlugin` interface (or abstract base class) exists in the backend defining:
      `PluginId`, `FetchAsync()`, `ParseArticle()`, and metadata (country, type, display name).
- [ ] Each existing government source (uae-gov, saudi-spa, qatar-qna, bahrain-bna) is implemented
      as a class implementing `ISourcePlugin`.
- [ ] Plugins are registered via `appsettings.json` or a dedicated `sources.json` config file —
      no code change required to enable/disable a plugin.
- [ ] Adding a new plugin only requires: (a) implementing `ISourcePlugin`, (b) adding an entry to
      the sources config. Core ingestion scheduler does not change.
- [ ] Unit tests exist for the plugin resolution/loading mechanism.

---

### US-007: Source plugin — ingest a tier-1 news outlet
**Description:** As a developer, I want RSS-based outlet plugins (FT, WSJ, NYT) that try archive.is
for full article text when the feed only contains a summary.

**Acceptance Criteria:**
- [ ] `RssSourcePlugin` base class handles RSS fetch and parse; outlet plugins extend it.
- [ ] If article body from RSS is < 200 words, plugin attempts to fetch
      `https://archive.is/newest/{article_url}` and parse article body from the result.
- [ ] If archive.is also fails or returns a paywall page, the RSS summary is stored and the article
      is flagged `full_text: false`.
- [ ] FT, WSJ, and NYT plugins implemented using `RssSourcePlugin`.
- [ ] Unit tests cover the archive.is fallback path.

---

### US-008: AI credibility scoring agent
**Description:** As the system, I want a Claude-powered multi-agent pipeline to evaluate each new
article and return a structured credibility score with reasoning so that readers receive transparent
quality signals.

**Acceptance Criteria:**
- [ ] A background job picks up newly ingested, unscored articles and sends them to the scoring
      pipeline.
- [ ] The pipeline uses at least two agents:
      1. **Extraction Agent** — extracts factual claims, named entities, and verifies internal
         consistency of the article.
      2. **Scoring Agent** — receives the extraction result plus source metadata and produces a
         score (integer 0–100) and `reasoning` string (2–4 sentences in the selected language).
- [ ] Scoring considers: source authority tier, presence of verifiable claims, cross-source
      corroboration (if ≥ 1 other plugin has published the same event), and recency.
- [ ] The score and reasoning are stored alongside the article record.
- [ ] If the Claude API call fails, the article is stored with `score: null` and retried on next
      scheduler run (max 3 attempts).
- [ ] Unit / integration tests validate the scoring pipeline with a mocked Claude client.

---

### US-009: Scheduled ingestion background service
**Description:** As the system, I want a scheduled background service to poll all registered source
plugins on a configurable interval so that the feed stays current without manual triggering.

**Acceptance Criteria:**
- [ ] Background service polls each plugin on a per-plugin configurable schedule (default: every
      15 minutes).
- [ ] Duplicate detection: if an article URL already exists in the database it is not re-inserted.
- [ ] Failed plugin polls are logged with source ID and error; they do not crash the service.
- [ ] Ingestion schedule is configurable via `appsettings.json` without redeployment.
- [ ] Unit tests cover duplicate detection logic.

---

### US-010: Search articles
**Description:** As a visitor, I want to search for articles by keyword so that I can find coverage
of a specific event or person.

**Acceptance Criteria:**
- [ ] Search input visible in the header on all pages.
- [ ] Returns articles whose headline or summary contains the keyword (case-insensitive).
- [ ] Results are displayed in the same card format as the main feed.
- [ ] Empty-state message shown when no results are found.
- [ ] Search query is reflected in the URL (`?q=aramco`).
- [ ] Verify in browser using dev-browser skill.

---

### US-011: .NET Aspire solution setup and local orchestration
**Description:** As a developer, I want the entire solution orchestrated by a .NET Aspire AppHost
so that I can run all services locally with a single `dotnet run --project AppHost` command and
observe them through the Aspire dashboard.

**Acceptance Criteria:**
- [ ] Solution contains `GulfInfoTracker.AppHost`, `GulfInfoTracker.ServiceDefaults`, `GulfInfoTracker.Api`,
      and `GulfInfoTracker.Web` projects.
- [ ] `dotnet run --project src/GulfInfoTracker.AppHost` starts the API, the React dev server,
      a local SQL Server container, and a local Redis container without manual setup.
- [ ] The Aspire dashboard is accessible at `https://localhost:15888` and shows all resources as
      healthy.
- [ ] The React frontend successfully calls the API using the Aspire-injected service URL (no
      hardcoded localhost ports).
- [ ] OTel traces from the API appear in the Aspire dashboard trace explorer.
- [ ] `azd up` from the repo root successfully deploys all resources to an Azure subscription
      using the Aspire-generated Bicep manifest.

---

### US-012: Admin source management page (internal, unauthenticated in MVP)
**Description:** As an operator, I want a simple admin page that shows the health of each source
plugin (last poll time, article count, error count) so that I can quickly diagnose ingestion problems.

**Acceptance Criteria:**
- [ ] `/admin/sources` route renders a table: Plugin ID, Display Name, Last Poll Time, Articles
      (24h), Last Error.
- [ ] Page is accessible without authentication in MVP (to be secured post-MVP).
- [ ] "Trigger Poll Now" button manually kicks off a poll for a given source.
- [ ] Verify in browser using dev-browser skill.

---

## Functional Requirements

- **FR-1:** The system must only ingest content from sources listed in the source configuration.
  No user-submitted or dynamically discovered URLs.
- **FR-2:** Every article stored must have: `id`, `pluginId`, `headline_en`, `headline_ar`,
  `summary_en`, `summary_ar`, `sourceUrl`, `publishedAt`, `ingestedAt`, `topicIds[]`, `country`,
  `credibilityScore` (nullable), `credibilityReasoning` (nullable), `fullText` (bool).
- **FR-3:** Arabic translation must be performed by the Claude API (translate-only prompt) if the
  source article is in English, and vice versa. A `translated: true` flag must be stored.
- **FR-4:** Credibility tiers are: Verified (80–100), Credible (60–79), Uncertain (40–59), Low
  (0–39). These are display labels derived from the numeric score — not stored separately.
- **FR-5:** The frontend must render correctly in RTL mode when Arabic is selected.
- **FR-6:** All API responses must be paginated with a consistent envelope:
  `{ data: [], total: number, page: number, pageSize: number }`.
- **FR-7:** The plugin interface must be documented with a `PLUGIN_GUIDE.md` explaining how to add
  a new source in ≤ 10 steps.
- **FR-8:** Cross-source corroboration: when scoring, the Scoring Agent must be provided with the
  count of distinct plugins that have published an article on the same event within ± 24 hours
  (matched by shared named entities).
- **FR-9:** The solution must be orchestrated by a `.NET Aspire` AppHost project
  (`GulfInfoTracker.AppHost`) that declares all resources: the ASP.NET Core API, the React frontend
  (as a Node.js/Vite resource), Azure SQL Database, and Azure Cache for Redis. Service-to-service
  URLs and connection strings must be injected by Aspire — not hardcoded. Deployment to Azure is
  performed via `azd up`, targeting Azure Container Apps.
- **FR-10:** All environment secrets (Claude API key, connection strings) must be stored in Azure
  Key Vault, referenced from the Aspire AppHost manifest so `azd` provisions and links them
  automatically. Secrets must never appear in `appsettings.json` or in source control.
- **FR-11:** A `GulfInfoTracker.ServiceDefaults` project must be referenced by the API project and
  apply shared Aspire service defaults: OpenTelemetry traces + metrics, standardised health-check
  endpoints (`/health/live`, `/health/ready`), and resilience defaults.

---

## Non-Goals (Out of Scope for v1)

- User accounts, saved searches, or personalised feeds.
- Push notifications or email alerts.
- Social sharing features.
- Full-text scraping of government portals beyond what RSS/official API provides.
- Real-time WebSocket feed updates (polling is acceptable for v1).
- Moderation or editorial override of AI scores.
- Mobile native apps (iOS / Android) — web only.
- Manual Docker Compose setup — Aspire handles local container orchestration.
- Any content from non-allowlisted sources.
- Automated fact-checking against external databases (e.g. Wikidata, GDELT).
- Paid API subscriptions for FT / WSJ / NYT.

---

## Design Considerations

### Mobile-First Layout
- Single-column card feed on mobile; optional two-column grid on tablet+.
- Sticky header with search, language toggle, and topic/country filter bar.
- Article detail opens as a full-screen slide-up sheet on mobile.
- Minimum tap target: 44 × 44 px.

### Credibility Score Badge
- Colour-coded pill: green (Verified), blue (Credible), amber (Uncertain), red (Low).
- Numeric score + tier label always visible on card without interaction.

### RTL Support
- Use CSS logical properties (`margin-inline-start` etc.) throughout.
- Swap font to a clean Arabic-supporting typeface (e.g. IBM Plex Arabic or Noto Sans Arabic) when
  AR is active.
- All icons and chevrons must flip appropriately.

### Loading States
- Skeleton cards during feed load.
- Spinner overlay on language switch while translations load.

---

## Technical Considerations

### Solution Structure (.NET Aspire)
```
GulfInfoTracker.sln
├── src/
│   ├── GulfInfoTracker.AppHost/        # Aspire orchestrator — declares all resources
│   ├── GulfInfoTracker.ServiceDefaults/ # Shared OTel, health checks, resilience
│   ├── GulfInfoTracker.Api/            # ASP.NET Core Web API
│   ├── GulfInfoTracker.Web/            # React + Vite frontend (Node.js resource in Aspire)
│   └── GulfInfoTracker.Plugins/        # ISourcePlugin implementations
└── tests/
    ├── GulfInfoTracker.Api.Tests/
    └── GulfInfoTracker.Plugins.Tests/
```

### .NET Aspire AppHost
- The AppHost (`Program.cs`) declares resources using the Aspire hosting API:
  ```csharp
  var sql   = builder.AddAzureSqlDatabase("sql");
  var redis = builder.AddAzureRedis("redis");
  var api   = builder.AddProject<Projects.GulfInfoTracker_Api>("api")
                     .WithReference(sql)
                     .WithReference(redis);
  var web   = builder.AddNpmApp("web", "../GulfInfoTracker.Web")
                     .WithReference(api)
                     .WithHttpEndpoint(env: "PORT");
  ```
- In local development, Aspire spins up SQL Server and Redis via Docker containers automatically.
- `azd up` reads the Aspire manifest and provisions Azure Container Apps, Azure SQL, Azure Cache
  for Redis, Azure Container Registry, and Azure Key Vault.
- The Aspire developer dashboard (localhost) provides real-time logs, traces, and resource health
  during local development — no separate Seq or Jaeger setup required.

### Backend (ASP.NET Core — `GulfInfoTracker.Api`)
- References `GulfInfoTracker.ServiceDefaults` to apply OTel, health checks, and resilience.
- **Plugin system:** `ISourcePlugin` resolved via .NET DI; plugins registered in `sources.json`
  config (not hardcoded in `Program.cs`).
- **Background services:** `IHostedService` + `PeriodicTimer` for ingestion scheduler.
- **Claude integration:** Anthropic .NET SDK (or raw `HttpClient` if SDK unavailable); wrapped in
  `ICredibilityPipeline` for test mocking.
- **Database:** Entity Framework Core with Azure SQL via `Aspire.Microsoft.EntityFrameworkCore.SqlServer`
  integration package; migrations committed to repo.
- **Caching:** Redis via `Aspire.StackExchange.Redis`; 5-minute TTL on feed endpoint; invalidated
  on new article insert.
- **Translation:** Claude API with a dedicated translate-only system prompt; results cached in DB.

### Frontend (React — `GulfInfoTracker.Web`)
- **Build tool:** Vite.
- **Routing:** React Router v6.
- **State:** TanStack Query for server state; no global state library needed for v1.
- **i18n:** `react-i18next` for UI strings; article content served pre-translated from API.
- **Styling:** Tailwind CSS with CSS logical properties for RTL support.
- **Testing:** Vitest + React Testing Library.
- Aspire injects the backend API base URL via the `services__api__https__0` environment variable
  (standard Aspire service-binding convention); Vite exposes this as `VITE_API_URL`.

### Azure Deployment (via `azd` + Aspire manifest)
| Resource                       | SKU (dev)     | Notes                                     |
|--------------------------------|---------------|-------------------------------------------|
| Azure Container Apps Env       | Consumption   | Hosts API and Web containers              |
| Azure Container Registry       | Basic         | Stores built images                       |
| Azure SQL Database             | Basic         | Upgrade to Standard for production        |
| Azure Cache for Redis          | C0 Basic      | Feed cache                                |
| Azure Key Vault                | Standard      | Claude API key, connection strings        |
| Azure Monitor / App Insights   | Pay-as-you-go | Receives OTel traces + metrics from Aspire|

- `azd` provisions all resources from the Aspire-generated manifest; no manual ARM/Bicep authoring
  required for initial deployment (Aspire generates Bicep under the hood).
- CI/CD via GitHub Actions: `azd pipeline config` scaffolds the workflow automatically.
- No separate Docker Compose file needed — Aspire replaces it for local development.

### API Shape (key endpoints)
```
GET  /api/articles?page=1&pageSize=20&topic=T3&country=uae&q=aramco
GET  /api/articles/{id}
GET  /api/sources            (plugin health summary)
POST /api/sources/{id}/poll  (manual trigger)
GET  /api/health
```

---

## Success Metrics

| Metric                                           | Target             |
|--------------------------------------------------|--------------------|
| Mobile Lighthouse Performance score              | ≥ 80               |
| Feed initial load time (mobile 3G)               | < 3 seconds        |
| Credibility score coverage (articles scored)     | ≥ 95 % within 5 min of ingestion |
| Source plugin onboarding time (new source)       | < 2 hours for a developer familiar with the codebase |
| Arabic translation availability                  | 100 % of articles have both EN and AR versions |
| Uptime SLA                                       | ≥ 99.5 % (monthly) |

---

## Open Questions

1. **Conflict topic auto-tagging:** The Iran/Israel/US Conflict topic (T5) is cross-regional.
   Should articles from non-Gulf sources (e.g. NYT) covering this topic also appear, or only when
   there is a direct Gulf-state angle?

2. **archive.is legality / ToS:** Confirm that using archive.is as a fallback for FT/WSJ/NYT
   content is acceptable under the project's legal review before going to production.

3. **Cross-source corroboration matching:** Named-entity matching for corroboration (FR-8) requires
   NER. Should this be handled by Claude extraction or a lightweight local NER library?

4. **Arabic translation quality threshold:** If Claude returns a translation with low confidence,
   should the article be held back or published with a prominent "translation unverified" warning?

5. **Admin page security:** `/admin/sources` is unauthenticated in v1. What is the timeline for
   adding Azure AD / Entra ID protection before this application is public-facing?

6. **Data retention:** How long should articles be retained? Is there a storage or compliance
   requirement for archiving Gulf government statements?
