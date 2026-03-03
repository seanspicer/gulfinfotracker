using System.ServiceModel.Syndication;
using System.Xml;
using GulfInfoTracker.Plugins.Abstractions;
using HtmlAgilityPack;

namespace GulfInfoTracker.Plugins.News;

public abstract class RssSourcePlugin(SourceConfig config, HttpClient http) : ISourcePlugin
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

        var results = new List<RawArticle>();
        foreach (var item in feed.Items)
        {
            var url = item.Links.FirstOrDefault()?.Uri.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(url)) continue;

            var body = ExtractBodyFromItem(item);

            if (WordCount(body) < 200)
            {
                body = await TryArchiveIsFallbackAsync(url, ct) ?? body;
            }

            results.Add(new RawArticle(
                Url: url,
                HeadlineEn: item.Title?.Text ?? string.Empty,
                HeadlineAr: null,
                BodyText: body,
                PublishedAt: item.PublishDate.UtcDateTime == default ? DateTime.UtcNow : item.PublishDate.UtcDateTime,
                OriginalLanguage: "en"
            ));
        }
        return results;
    }

    public ArticleCandidate? ParseArticle(RawArticle raw)
    {
        if (string.IsNullOrWhiteSpace(raw.Url)) return null;

        return new ArticleCandidate(
            PluginId: config.PluginId,
            HeadlineEn: raw.HeadlineEn,
            HeadlineAr: null,
            SummaryEn: raw.BodyText,
            SummaryAr: null,
            SourceUrl: raw.Url,
            PublishedAt: raw.PublishedAt,
            Country: config.Country,
            FullText: WordCount(raw.BodyText ?? string.Empty) >= 200,
            OriginalLanguage: raw.OriginalLanguage
        );
    }

    protected virtual string ExtractBodyFromItem(SyndicationItem item) =>
        System.Text.RegularExpressions.Regex.Replace(item.Summary?.Text ?? string.Empty, "<[^>]+>", " ").Trim();

    // TODO: Legal/ToS review required before production use
    protected async Task<string?> TryArchiveIsFallbackAsync(string url, CancellationToken ct)
    {
        try
        {
            var archiveUrl = $"https://archive.is/newest/{Uri.EscapeDataString(url)}";
            var response = await http.GetAsync(archiveUrl, ct);
            if (!response.IsSuccessStatusCode) return null;

            var html = await response.Content.ReadAsStringAsync(ct);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var article = doc.DocumentNode.SelectSingleNode("//article")
                ?? doc.DocumentNode.SelectSingleNode("//*[@id='article']");

            if (article is null) return null;

            var text = article.InnerText;
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
            return text.Length > 0 ? text : null;
        }
        catch
        {
            // Non-fatal: degrade gracefully to FullText: false
            return null;
        }
    }

    protected static int WordCount(string text) =>
        string.IsNullOrWhiteSpace(text) ? 0 : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
}
