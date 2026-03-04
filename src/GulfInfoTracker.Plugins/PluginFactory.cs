using GulfInfoTracker.Plugins.Abstractions;
using GulfInfoTracker.Plugins.Government;
using GulfInfoTracker.Plugins.News;

namespace GulfInfoTracker.Plugins;

public class PluginFactory(IEnumerable<SourceConfig> configs, IHttpClientFactory httpClientFactory) : IPluginFactory
{
    public IReadOnlyList<ISourcePlugin> GetEnabledPlugins()
    {
        var result = new List<ISourcePlugin>();
        foreach (var cfg in configs.Where(c => c.Enabled))
        {
            var http = httpClientFactory.CreateClient(cfg.PluginId);
            ISourcePlugin? plugin = cfg.PluginId switch
            {
                // UAE
                "gulf-news"        => new GulfNewsPlugin(cfg, http),
                "khaleej-times"    => new KhaleejTimesPlugin(cfg, http),
                "the-national"     => new TheNationalPlugin(cfg, http),
                "arabian-business" => new ArabianBusinessPlugin(cfg, http),
                "uae-ncema"        => new UaeNcemaPlugin(cfg, http),
                "uae-mod"          => new UaeModPlugin(cfg, http),

                // Qatar
                "qatar-qna" or
                "qatar-qna-economy" => new QatarQnaPlugin(cfg, http),
                "al-jazeera"        => new AlJazeeraPlugin(cfg, http),

                // Kuwait
                "kuwait-times" => new KuwaitTimesPlugin(cfg, http),

                // Saudi Arabia
                "saudi-gazette" or
                "saudi-gazette-economy" => new SaudiGazettePlugin(cfg, http),

                // Oman
                "oman-ona" or
                "oman-ona-economy" => new OmanOnaPlugin(cfg, http),
                "times-of-oman"    => new TimesOfOmanPlugin(cfg, http),
                "muscat-daily"     => new MuscatDailyPlugin(cfg, http),

                // Bahrain
                "bahrain-bna" or
                "bahrain-bna-business" => new BahrainBnaPlugin(cfg, http),

                // International
                "bloomberg-markets" or
                "bloomberg-economics" => new BloombergPlugin(cfg, http),
                "wsj" => new WsjPlugin(cfg, http),
                "nyt" => new NytPlugin(cfg, http),

                _ => null
            };
            if (plugin is not null) result.Add(plugin);
        }
        return result;
    }
}
