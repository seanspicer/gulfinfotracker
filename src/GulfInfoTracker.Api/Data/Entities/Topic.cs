namespace GulfInfoTracker.Api.Data.Entities;

public class Topic
{
    public required string Id { get; set; }   // T1–T5
    public required string LabelEn { get; set; }
    public required string LabelAr { get; set; }

    public ICollection<ArticleTopic> ArticleTopics { get; set; } = [];
}
