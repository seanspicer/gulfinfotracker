namespace GulfInfoTracker.Plugins.Abstractions;

public record ArticleCandidate(
    string PluginId,
    string HeadlineEn,
    string? HeadlineAr,
    string? SummaryEn,
    string? SummaryAr,
    string SourceUrl,
    DateTime PublishedAt,
    string Country,
    bool FullText,
    string OriginalLanguage
);
