using GulfInfoTracker.Plugins;

namespace GulfInfoTracker.Api.Services;

/// <summary>
/// Background service that polls each registered source plugin on a configurable interval.
/// One PeriodicTimer loop per plugin; all loops run concurrently.
/// Uses IServiceScopeFactory to resolve scoped IIngestionProcessor per poll (singleton-safe).
/// </summary>
public class IngestionService(
    IPluginFactory pluginFactory,
    IServiceScopeFactory scopeFactory,
    ILogger<IngestionService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var plugins = pluginFactory.GetEnabledPlugins();
        if (plugins.Count == 0)
        {
            logger.LogWarning("No enabled plugins found. Ingestion service is idle.");
            return;
        }

        logger.LogInformation("Starting ingestion service with {Count} plugin(s).", plugins.Count);

        var tasks = plugins.Select(plugin => RunPluginLoopAsync(plugin.PluginId, stoppingToken));
        await Task.WhenAll(tasks);
    }

    private async Task RunPluginLoopAsync(string pluginId, CancellationToken ct)
    {
        var plugin = pluginFactory.GetEnabledPlugins().FirstOrDefault(p => p.PluginId == pluginId);
        if (plugin is null) return;

        await PollAsync(pluginId, ct);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));
        while (await timer.WaitForNextTickAsync(ct))
        {
            await PollAsync(pluginId, ct);
        }
    }

    private async Task PollAsync(string pluginId, CancellationToken ct)
    {
        var plugin = pluginFactory.GetEnabledPlugins().FirstOrDefault(p => p.PluginId == pluginId);
        if (plugin is null) return;

        await using var scope = scopeFactory.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<IIngestionProcessor>();
        await processor.IngestPluginAsync(plugin, ct);
    }
}
