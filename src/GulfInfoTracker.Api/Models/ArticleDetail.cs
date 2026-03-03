namespace GulfInfoTracker.Api.Models;

public record ArticleDetail(
    Guid Id,
    string PluginId,
    string HeadlineEn,
    string? HeadlineAr,
    string? SummaryEn,
    string? SummaryAr,
    string SourceUrl,
    DateTime PublishedAt,
    DateTime IngestedAt,
    string Country,
    int? CredibilityScore,
    string? CredibilityReasoning,
    bool FullText,
    bool Translated,
    IReadOnlyList<string> TopicIds
);
