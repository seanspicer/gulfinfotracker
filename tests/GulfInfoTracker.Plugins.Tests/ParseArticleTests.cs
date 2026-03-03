using GulfInfoTracker.Plugins.Abstractions;
using GulfInfoTracker.Plugins.Government;
using NUnit.Framework;

namespace GulfInfoTracker.Plugins.Tests;

[TestFixture]
public class ParseArticleTests
{
    private static UaeGovPlugin CreatePlugin() =>
        new(new SourceConfig("uae-gov", "UAE Gov", "UAE", "government", true, 15, "https://example.com"),
            new HttpClient());

    [Test]
    public void ParseArticle_NullUrl_ReturnsNull()
    {
        var plugin = CreatePlugin();
        var raw = new RawArticle("", "Headline", null, "Body", DateTime.UtcNow, "en");
        Assert.That(plugin.ParseArticle(raw), Is.Null);
    }

    [Test]
    public void ParseArticle_ValidRaw_MapsFieldsCorrectly()
    {
        var plugin = CreatePlugin();
        var raw = new RawArticle(
            "https://example.com/article",
            "Test Headline",
            null,
            "Short body text.",
            DateTime.UtcNow,
            "en");

        var candidate = plugin.ParseArticle(raw);

        Assert.That(candidate, Is.Not.Null);
        Assert.That(candidate!.HeadlineEn, Is.EqualTo("Test Headline"));
        Assert.That(candidate.Country, Is.EqualTo("UAE"));
        Assert.That(candidate.PluginId, Is.EqualTo("uae-gov"));
    }
}
