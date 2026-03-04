using GulfInfoTracker.Plugins;
using GulfInfoTracker.Plugins.Abstractions;
using System.Text.Json;

namespace GulfInfoTracker.Api.Extensions;

public static class PluginServiceExtensions
{
    // Hosts whose TLS certificates cannot be validated (e.g. self-signed gov certs).
    private static readonly HashSet<string> SslBypassHosts = ["omannews.gov.om", "www.ncema.gov.ae"];

    public static IServiceCollection AddPlugins(this IServiceCollection services, string sourcesJsonPath, IConfiguration config)
    {
        List<SourceConfig> configs = [];
        if (File.Exists(sourcesJsonPath))
        {
            var json = File.ReadAllText(sourcesJsonPath);
            configs = JsonSerializer.Deserialize<List<SourceConfig>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }

        var xBearerToken = config["X:BearerToken"] ?? config["X_BEARER_TOKEN"];

        foreach (var cfg in configs)
        {
            services.AddSingleton(cfg);

            if (cfg.Type == "x")
            {
                // X API v2 — bearer token auth, no RSS headers
                services.AddHttpClient(cfg.PluginId).ConfigureHttpClient(c =>
                {
                    c.Timeout = TimeSpan.FromSeconds(30);
                    if (!string.IsNullOrWhiteSpace(xBearerToken))
                        c.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", xBearerToken);
                });
                continue;
            }

            var host = new Uri(cfg.FeedUrl).Host;
            var builder = services.AddHttpClient(cfg.PluginId).ConfigureHttpClient(c =>
            {
                c.Timeout = TimeSpan.FromSeconds(30);
                c.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (compatible; GulfInfoTracker/1.0; +https://github.com/your-org/gulf-info-tracker)");
                c.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/atom+xml, application/xml, text/xml;q=0.9");
            });

            if (SslBypassHosts.Contains(host))
            {
                builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            }
        }

        services.AddSingleton<IPluginFactory, PluginFactory>();
        return services;
    }
}
