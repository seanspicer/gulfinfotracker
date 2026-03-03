using System.ServiceModel.Syndication;
using System.Xml;
using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Government;

public abstract class GovRssPlugin(SourceConfig config, HttpClient http) : ISourcePlugin
{
    public string PluginId => config.PluginId;
    public string DisplayName => config.DisplayName;
    public string Country => config.Country;
    public string Type => config.Type;

    public async Task<IReadOnlyList<RawArticle>> FetchAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync(config.FeedUrl, ct);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);

        var xmlSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
        using var reader = XmlReader.Create(stream, xmlSettings);
        var feed = SyndicationFeed.Load(reader);

        return feed.Items.Select(item => new RawArticle(
            Url: item.Links.FirstOrDefault()?.Uri.ToString() ?? string.Empty,
            HeadlineEn: item.Title?.Text ?? string.Empty,
            HeadlineAr: null,
            BodyText: TrimToSentenceBoundary(item.Summary?.Text ?? string.Empty, 500),
            PublishedAt: item.PublishDate.UtcDateTime == default ? DateTime.UtcNow : item.PublishDate.UtcDateTime,
            OriginalLanguage: "en"
        )).ToList();
    }

    public ArticleCandidate? ParseArticle(RawArticle raw)
    {
        if (string.IsNullOrWhiteSpace(raw.Url)) return null;

        return new ArticleCandidate(
            PluginId: config.PluginId,
            HeadlineEn: raw.HeadlineEn,
            HeadlineAr: raw.HeadlineAr,
            SummaryEn: raw.BodyText,
            SummaryAr: null,
            SourceUrl: raw.Url,
            PublishedAt: raw.PublishedAt,
            Country: config.Country,
            FullText: false,
            OriginalLanguage: raw.OriginalLanguage
        );
    }

    protected static string TrimToSentenceBoundary(string text, int maxChars)
    {
        // Strip HTML tags first (basic)
        text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", " ").Trim();
        if (text.Length <= maxChars) return text;

        var trimmed = text[..maxChars];
        var lastPeriod = trimmed.LastIndexOfAny(['.', '!', '?']);
        return lastPeriod > 0 ? trimmed[..(lastPeriod + 1)] : trimmed;
    }
}
