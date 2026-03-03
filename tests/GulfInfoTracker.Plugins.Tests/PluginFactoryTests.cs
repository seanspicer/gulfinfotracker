using GulfInfoTracker.Plugins;
using GulfInfoTracker.Plugins.Abstractions;
using Moq;
using NUnit.Framework;

namespace GulfInfoTracker.Plugins.Tests;

[TestFixture]
public class PluginFactoryTests
{
    private PluginFactory CreateFactory(IEnumerable<SourceConfig> configs)
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient());
        return new PluginFactory(configs, httpClientFactoryMock.Object);
    }

    [Test]
    public void GetEnabledPlugins_EnabledPlugin_IsReturned()
    {
        var configs = new[]
        {
            new SourceConfig("uae-gov", "UAE Gov", "UAE", "government", true, 15, "https://example.com/rss")
        };
        var factory = CreateFactory(configs);

        var plugins = factory.GetEnabledPlugins();

        Assert.That(plugins, Has.Count.EqualTo(1));
        Assert.That(plugins[0].PluginId, Is.EqualTo("uae-gov"));
    }

    [Test]
    public void GetEnabledPlugins_DisabledPlugin_IsExcluded()
    {
        var configs = new[]
        {
            new SourceConfig("uae-gov", "UAE Gov", "UAE", "government", false, 15, "https://example.com/rss")
        };
        var factory = CreateFactory(configs);

        var plugins = factory.GetEnabledPlugins();

        Assert.That(plugins, Is.Empty);
    }

    [Test]
    public void GetEnabledPlugins_MultiplePlugins_OnlyEnabledReturned()
    {
        var configs = new[]
        {
            new SourceConfig("uae-gov",    "UAE Gov",    "UAE",  "government", true,  15, "https://example.com/rss"),
            new SourceConfig("saudi-spa",  "Saudi SPA",  "SA",   "government", false, 15, "https://example.com/rss"),
            new SourceConfig("ft",         "FT",         "INTL", "news",       true,  30, "https://example.com/rss"),
        };
        var factory = CreateFactory(configs);

        var plugins = factory.GetEnabledPlugins();

        Assert.That(plugins, Has.Count.EqualTo(2));
        Assert.That(plugins.Select(p => p.PluginId), Does.Not.Contain("saudi-spa"));
    }
}
