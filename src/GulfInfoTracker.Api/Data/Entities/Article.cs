namespace GulfInfoTracker.Api.Data.Entities;

public class Article
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string PluginId { get; set; }
    public required string HeadlineEn { get; set; }
    public string? HeadlineAr { get; set; }
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public required string SourceUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
    public required string Country { get; set; }
    public int? CredibilityScore { get; set; }
    public string? CredibilityReasoning { get; set; }
    public bool FullText { get; set; }
    public bool Translated { get; set; }
    public int ScoringAttempts { get; set; }
    public string? NamedEntitiesJson { get; set; }

    public ICollection<ArticleTopic> ArticleTopics { get; set; } = [];
}
