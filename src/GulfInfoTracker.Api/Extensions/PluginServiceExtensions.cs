using GulfInfoTracker.Plugins;
using GulfInfoTracker.Plugins.Abstractions;
using System.Text.Json;

namespace GulfInfoTracker.Api.Extensions;

public static class PluginServiceExtensions
{
    public static IServiceCollection AddPlugins(this IServiceCollection services, string sourcesJsonPath)
    {
        List<SourceConfig> configs = [];
        if (File.Exists(sourcesJsonPath))
        {
            var json = File.ReadAllText(sourcesJsonPath);
            configs = JsonSerializer.Deserialize<List<SourceConfig>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }

        foreach (var cfg in configs)
        {
            services.AddSingleton(cfg);
            services.AddHttpClient(cfg.PluginId).ConfigureHttpClient(c =>
            {
                c.Timeout = TimeSpan.FromSeconds(30);
                c.DefaultRequestHeaders.UserAgent.ParseAdd("GulfInfoTracker/1.0");
            });
        }

        services.AddSingleton<IPluginFactory, PluginFactory>();
        return services;
    }
}
