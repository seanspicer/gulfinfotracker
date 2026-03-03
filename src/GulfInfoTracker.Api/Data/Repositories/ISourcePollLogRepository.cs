using GulfInfoTracker.Api.Data.Entities;

namespace GulfInfoTracker.Api.Data.Repositories;

public record SourceHealthSummary(
    string PluginId,
    DateTime? LastPolledAt,
    int ArticlesLast24h,
    string? LastError
);

public interface ISourcePollLogRepository
{
    Task LogPollAsync(string pluginId, bool success, int articlesIngested, string? errorMessage, CancellationToken ct = default);
    Task<IReadOnlyList<SourceHealthSummary>> GetHealthSummaryAsync(CancellationToken ct = default);
}
