namespace GulfInfoTracker.Plugins.Abstractions;

public record SourceConfig(
    string PluginId,
    string DisplayName,
    string Country,
    string Type,
    bool Enabled,
    int PollIntervalMinutes,
    string FeedUrl
);
