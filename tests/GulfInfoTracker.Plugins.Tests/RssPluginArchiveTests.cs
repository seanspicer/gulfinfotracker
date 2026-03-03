using System.Net;
using System.Net.Http;
using GulfInfoTracker.Plugins.Abstractions;
using GulfInfoTracker.Plugins.News;
using NUnit.Framework;

namespace GulfInfoTracker.Plugins.Tests;

[TestFixture]
public class RssPluginArchiveTests
{
    // RSS feed with short body (< 200 words) that triggers archive.is fallback
    private const string ShortRssFeed = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0">
          <channel>
            <title>Test</title>
            <item>
              <title>Test Article</title>
              <link>https://example.com/article</link>
              <description>Short summary.</description>
              <pubDate>Mon, 03 Mar 2025 12:00:00 GMT</pubDate>
            </item>
          </channel>
        </rss>
        """;

    private const string ValidArchiveHtml = """
        <html><body><article>This is the full article text from archive. It contains many words to demonstrate that the archive fallback works correctly and provides the full text content that was not available in the RSS feed summary. The article discusses various topics including finance, economics, and world affairs in a comprehensive manner. The content is substantial and informative, providing readers with a complete understanding of the subject matter discussed. The Gulf region continues to experience rapid economic transformation as governments implement ambitious diversification strategies. Saudi Arabia and the United Arab Emirates lead the way with major investment programs spanning technology, tourism, and renewable energy sectors. These initiatives represent a fundamental shift away from traditional oil-dependent economies toward more sustainable and diversified growth models. Analysts suggest that the pace of reform has accelerated considerably in recent years, driven by changing global energy markets and increasing awareness of environmental challenges. The strategic vision documents published by several Gulf states outline detailed roadmaps for economic development through the year twenty thirty and beyond. Infrastructure projects worth hundreds of billions of dollars are currently underway across the region, creating new opportunities for international investors and businesses looking to establish a presence in these growing markets. Regional cooperation has also strengthened, with new trade agreements and joint ventures bringing Gulf economies closer together than ever before. The financial services sector has emerged as a key pillar of growth.</article></body></html>
        """;

    [Test]
    public async Task FetchAsync_ShortBody_TriesArchiveIs()
    {
        bool archiveCalled = false;
        var handler = new TrackingMockHandler(
            ShortRssFeed,
            archiveHtml: ValidArchiveHtml,
            onArchiveCalled: () => archiveCalled = true);

        var config = new SourceConfig("ft", "FT", "INTL", "news", true, 30, "https://example.com/rss");
        var plugin = new FtPlugin(config, new HttpClient(handler));

        var articles = await plugin.FetchAsync();

        Assert.That(archiveCalled, Is.True);
    }

    [Test]
    public async Task FetchAsync_ShortBody_ArchiveReturnsHtml_FullTextTrue()
    {
        var handler = new TrackingMockHandler(ShortRssFeed, archiveHtml: ValidArchiveHtml, onArchiveCalled: () => { });
        var config = new SourceConfig("ft", "FT", "INTL", "news", true, 30, "https://example.com/rss");
        var plugin = new FtPlugin(config, new HttpClient(handler));

        var articles = await plugin.FetchAsync();
        var candidate = plugin.ParseArticle(articles[0]);

        Assert.That(candidate!.FullText, Is.True);
    }

    [Test]
    public async Task FetchAsync_ShortBody_ArchiveThrows_ArticleStoredWithFullTextFalse()
    {
        var handler = new FailingArchiveHandler(ShortRssFeed);
        var config = new SourceConfig("ft", "FT", "INTL", "news", true, 30, "https://example.com/rss");
        var plugin = new FtPlugin(config, new HttpClient(handler));

        IReadOnlyList<RawArticle> articles = null!;
        Assert.DoesNotThrowAsync(async () => articles = await plugin.FetchAsync());
        Assert.That(articles, Has.Count.EqualTo(1));
        var candidate = plugin.ParseArticle(articles[0]);
        Assert.That(candidate!.FullText, Is.False);
    }
}

// Helper mock handlers
internal class TrackingMockHandler(string rssFeed, string archiveHtml, Action onArchiveCalled) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri!.Host == "archive.is")
        {
            onArchiveCalled();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(archiveHtml, System.Text.Encoding.UTF8, "text/html")
            });
        }
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(rssFeed, System.Text.Encoding.UTF8, "application/rss+xml")
        });
    }
}

internal class FailingArchiveHandler(string rssFeed) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri!.Host == "archive.is")
            throw new HttpRequestException("Archive.is unavailable");
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(rssFeed, System.Text.Encoding.UTF8, "application/rss+xml")
        });
    }
}

internal class MockHttpMessageHandler((string UrlPart, HttpStatusCode Code, string Content)[] responses) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var match = responses.FirstOrDefault(r => request.RequestUri?.ToString().Contains(r.UrlPart) == true);
        if (match == default) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        return Task.FromResult(new HttpResponseMessage(match.Code)
        {
            Content = new StringContent(match.Content, System.Text.Encoding.UTF8, "text/plain")
        });
    }
}
