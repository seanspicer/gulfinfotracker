namespace GulfInfoTracker.Api.Data.Entities;

public class ArticleTopic
{
    public Guid ArticleId { get; set; }
    public required string TopicId { get; set; }

    public Article Article { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
}
