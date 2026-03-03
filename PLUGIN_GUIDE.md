# Plugin Guide — Adding a New Source

This guide walks you through adding a new content source to Gulf Info Tracker in ≤10 steps. No changes to core ingestion or scoring logic are required.

## Prerequisites

- .NET 9 SDK
- The new source must have a public RSS feed or API endpoint
- You are familiar with the existing plugin structure in `src/GulfInfoTracker.Plugins/`

---

## Steps

### 1. Choose a plugin type

- **Government RSS source** → extend `GovRssPlugin` (in `src/GulfInfoTracker.Plugins/Government/`)
- **News outlet with archive.is fallback** → extend `RssSourcePlugin` (in `src/GulfInfoTracker.Plugins/News/`)
- **Custom API** → implement `ISourcePlugin` directly

### 2. Create a new class

**Example — adding a Kuwaiti news agency (KUNA):**

```csharp
// src/GulfInfoTracker.Plugins/Government/KunaPlugin.cs
using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Government;

public class KunaPlugin(SourceConfig config, HttpClient http) : GovRssPlugin(config, http);
```

For a news outlet with paywall articles:
```csharp
// src/GulfInfoTracker.Plugins/News/BloombergPlugin.cs
using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class BloombergPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
```

### 3. Add a `sources.json` entry

Open `sources.json` at the repository root and add a new entry:

```json
{
  "pluginId": "kuna",
  "displayName": "Kuwait News Agency",
  "country": "KW",
  "type": "government",
  "enabled": true,
  "pollIntervalMinutes": 15,
  "feedUrl": "https://www.kuna.net.kw/rss.aspx"
}
```

| Field | Description |
|-------|-------------|
| `pluginId` | Unique identifier. Use lowercase with hyphens. |
| `displayName` | Human-readable name shown in the admin UI. |
| `country` | ISO country code (UAE, SA, QA, BH, KW, …) or INTL. |
| `type` | `"government"` or `"news"` |
| `enabled` | Set to `false` to temporarily disable without removing. |
| `pollIntervalMinutes` | How often to poll (default: 15 for gov, 30 for news). |
| `feedUrl` | Full URL to the RSS/Atom feed. |

### 4. Register the plugin in PluginFactory

Open `src/GulfInfoTracker.Plugins/PluginFactory.cs` and add a `case` to the `switch` expression:

```csharp
"kuna" => new KunaPlugin(cfg, http),
```

### 5. Add a `using` if needed

If the plugin class is in a new namespace, add the `using` to `PluginFactory.cs`.

### 6. (Optional) Override `ExtractBodyFromItem`

If the RSS feed uses a non-standard content element, override the extraction method:

```csharp
public class KunaPlugin(SourceConfig config, HttpClient http) : GovRssPlugin(config, http)
{
    protected override string ExtractBodyFromItem(System.ServiceModel.Syndication.SyndicationItem item)
    {
        // Custom extraction logic
        var content = item.ElementExtensions.FirstOrDefault(e => e.OuterName == "content");
        return content?.GetObject<string>() ?? base.ExtractBodyFromItem(item);
    }
}
```

### 7. Write unit tests

Add tests in `tests/GulfInfoTracker.Plugins.Tests/`:

```csharp
[Test]
public void ParseArticle_KunaPlugin_MapsCountryCorrectly()
{
    var plugin = new KunaPlugin(
        new SourceConfig("kuna", "KUNA", "KW", "government", true, 15, "https://example.com"),
        new HttpClient());

    var raw = new RawArticle("https://kuna.net.kw/1", "Headline", null, "Body", DateTime.UtcNow, "en");
    var candidate = plugin.ParseArticle(raw);

    Assert.That(candidate!.Country, Is.EqualTo("KW"));
}
```

### 8. Build and test

```bash
dotnet build GulfInfoTracker.sln
dotnet test tests/GulfInfoTracker.Plugins.Tests/
```

### 9. Run locally

```bash
dotnet run --project src/GulfInfoTracker.AppHost
```

Open the Aspire dashboard at `https://localhost:15888`. The new plugin will be visible in the source health list at `/admin/sources`.

### 10. Deploy

Commit your changes and push to `main`. The GitHub Actions workflow will run tests and deploy via `azd deploy`.

---

## ISourcePlugin Interface Reference

```csharp
public interface ISourcePlugin
{
    string PluginId { get; }       // Unique ID matching sources.json
    string DisplayName { get; }    // Human-readable name
    string Country { get; }        // Country code
    string Type { get; }           // "government" or "news"
    Task<IReadOnlyList<RawArticle>> FetchAsync(CancellationToken ct = default);
    ArticleCandidate? ParseArticle(RawArticle raw);
}
```

`FetchAsync` fetches and returns raw articles from the source. `ParseArticle` maps a raw article to the canonical `ArticleCandidate` shape consumed by the ingestion pipeline. Return `null` to skip invalid articles.
