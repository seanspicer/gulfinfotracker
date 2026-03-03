using GulfInfoTracker.Api.Data.Repositories;
using GulfInfoTracker.Api.Models;
using GulfInfoTracker.Api.Services;
using GulfInfoTracker.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace GulfInfoTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController(
    IPluginFactory pluginFactory,
    ISourcePollLogRepository pollLogRepo,
    IIngestionProcessor ingestionProcessor) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SourceHealthDto>>> GetSources(CancellationToken ct = default)
    {
        var plugins = pluginFactory.GetEnabledPlugins();
        var healthSummaries = await pollLogRepo.GetHealthSummaryAsync(ct);
        var healthDict = healthSummaries.ToDictionary(h => h.PluginId);

        var dtos = plugins.Select(p =>
        {
            healthDict.TryGetValue(p.PluginId, out var health);
            return new SourceHealthDto(
                p.PluginId,
                p.DisplayName,
                health?.LastPolledAt,
                health?.ArticlesLast24h ?? 0,
                health?.LastError);
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost("{id}/poll")]
    public async Task<IActionResult> TriggerPoll(string id, CancellationToken ct = default)
    {
        var plugin = pluginFactory.GetEnabledPlugins().FirstOrDefault(p => p.PluginId == id);
        if (plugin is null) return NotFound($"Source '{id}' not found.");

        await ingestionProcessor.IngestPluginAsync(plugin, ct);
        return Ok(new { message = $"Poll triggered for '{id}'." });
    }
}
