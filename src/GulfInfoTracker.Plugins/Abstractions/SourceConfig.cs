namespace GulfInfoTracker.Plugins.Abstractions;

public record SourceConfig(
    string PluginId,
    string DisplayName,
    string Country,
    string Type,
    bool Enabled,
    int PollIntervalMinutes,
    string FeedUrl,
    string Language = "en"   // "en" or "ar" — used by plugins that can't auto-detect
);
