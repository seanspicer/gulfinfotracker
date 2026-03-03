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
                "uae-gov"     => new UaeGovPlugin(cfg, http),
                "saudi-spa"   => new SaudiSpaPlugin(cfg, http),
                "qatar-qna"   => new QatarQnaPlugin(cfg, http),
                "bahrain-bna" => new BahrainBnaPlugin(cfg, http),
                "ft"          => new FtPlugin(cfg, http),
                "wsj"         => new WsjPlugin(cfg, http),
                "nyt"         => new NytPlugin(cfg, http),
                _             => null
            };
            if (plugin is not null) result.Add(plugin);
        }
        return result;
    }
}
