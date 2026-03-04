using System.Globalization;
using System.Text.Json;
using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Social;

/// <summary>
/// Fetches recent tweets from a single X (Twitter) account via the X API v2 user timeline.
/// Requires a bearer token configured as X:BearerToken (or X_BEARER_TOKEN env var).
/// Rate limits: Basic tier — 5 requests per 15 min per app.
/// </summary>
public class XSourcePlugin(SourceConfig config, HttpClient http) : ISourcePlugin
{
    private readonly string _username = new Uri(config.FeedUrl).AbsolutePath.TrimStart('/');
    private string? _cachedUserId;

    public string PluginId    => config.PluginId;
    public string DisplayName => config.DisplayName;
    public string Country     => config.Country;
    public string Type        => config.Type;

    public async Task<IReadOnlyList<RawArticle>> FetchAsync(CancellationToken ct = default)
    {
        _cachedUserId ??= await ResolveUserIdAsync(ct);
        if (_cachedUserId is null) return [];

        var url = $"https://api.twitter.com/2/users/{_cachedUserId}/tweets" +
                  "?max_results=10&tweet.fields=created_at,text&exclude=retweets,replies";

        using var response = await http.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("data", out var data))
            return [];

        var results = new List<RawArticle>();
        foreach (var tweet in data.EnumerateArray())
        {
            var id        = tweet.GetProperty("id").GetString()!;
            var text      = tweet.GetProperty("text").GetString()!;
            var createdAt = tweet.TryGetProperty("created_at", out var ts)
                ? DateTime.Parse(ts.GetString()!, null, DateTimeStyles.RoundtripKind)
                : DateTime.UtcNow;

            // Skip Twitter card / media links that are just URLs with no meaningful text
            if (string.IsNullOrWhiteSpace(text) || text.StartsWith("https://t.co/"))
                continue;

            results.Add(new RawArticle(
                Url: $"https://x.com/{_username}/status/{id}",
                HeadlineEn: text,
                HeadlineAr: config.Language == "ar" ? text : null,
                BodyText: null,
                PublishedAt: createdAt,
                OriginalLanguage: config.Language
            ));
        }

        return results;
    }

    public ArticleCandidate? ParseArticle(RawArticle raw) => new(
        PluginId:          config.PluginId,
        HeadlineEn:        raw.HeadlineEn,
        HeadlineAr:        raw.HeadlineAr,
        SummaryEn:         raw.HeadlineEn,   // tweet text doubles as summary
        SummaryAr:         raw.HeadlineAr,
        SourceUrl:         raw.Url,
        PublishedAt:       raw.PublishedAt,
        Country:           config.Country,
        FullText:          false,
        OriginalLanguage:  raw.OriginalLanguage
    );

    private async Task<string?> ResolveUserIdAsync(CancellationToken ct)
    {
        try
        {
            using var response = await http.GetAsync(
                $"https://api.twitter.com/2/users/by/username/{_username}", ct);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("data").GetProperty("id").GetString();
        }
        catch
        {
            return null;
        }
    }
}
