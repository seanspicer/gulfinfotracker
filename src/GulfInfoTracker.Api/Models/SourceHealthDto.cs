namespace GulfInfoTracker.Api.Models;

public record SourceHealthDto(
    string PluginId,
    string DisplayName,
    DateTime? LastPolledAt,
    int ArticlesLast24h,
    string? LastError
);
