using GulfInfoTracker.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GulfInfoTracker.Api.Data.Repositories;

public class SourcePollLogRepository(AppDbContext db) : ISourcePollLogRepository
{
    public async Task LogPollAsync(string pluginId, bool success, int articlesIngested, string? errorMessage, CancellationToken ct = default)
    {
        db.SourcePollLogs.Add(new SourcePollLog
        {
            PluginId         = pluginId,
            Success          = success,
            ArticlesIngested = articlesIngested,
            ErrorMessage     = errorMessage,
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SourceHealthSummary>> GetHealthSummaryAsync(CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddHours(-24);

        var lastPolls = await db.SourcePollLogs
            .GroupBy(l => l.PluginId)
            .Select(g => new
            {
                PluginId      = g.Key,
                LastPolledAt  = g.Max(l => l.PolledAt),
                LastError     = g.OrderByDescending(l => l.PolledAt).First().ErrorMessage,
            })
            .ToListAsync(ct);

        var articleCounts = await db.Articles
            .Where(a => a.IngestedAt >= since)
            .GroupBy(a => a.PluginId)
            .Select(g => new { PluginId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countDict = articleCounts.ToDictionary(x => x.PluginId, x => x.Count);

        return lastPolls
            .Select(p => new SourceHealthSummary(
                p.PluginId,
                p.LastPolledAt,
                countDict.TryGetValue(p.PluginId, out var c) ? c : 0,
                p.LastError))
            .ToList();
    }
}
