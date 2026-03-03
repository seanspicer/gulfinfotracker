namespace GulfInfoTracker.Api.Data.Entities;

public class SourcePollLog
{
    public int Id { get; set; }
    public required string PluginId { get; set; }
    public DateTime PolledAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public int ArticlesIngested { get; set; }
    public string? ErrorMessage { get; set; }
}
