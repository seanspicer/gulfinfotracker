namespace GulfInfoTracker.Plugins.Abstractions;

public record RawArticle(
    string Url,
    string HeadlineEn,
    string? HeadlineAr,
    string? BodyText,
    DateTime PublishedAt,
    string OriginalLanguage   // "en" or "ar"
);
