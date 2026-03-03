namespace GulfInfoTracker.Api.Models;

public record ArticleListItem(
    Guid Id,
    string PluginId,
    string HeadlineEn,
    string? HeadlineAr,
    string? SummaryEn,
    string? SummaryAr,
    string SourceUrl,
    DateTime PublishedAt,
    string Country,
    int? CredibilityScore,
    bool FullText,
    bool Translated,
    IReadOnlyList<string> TopicIds
);
