using GulfInfoTracker.Api.Data.Entities;

namespace GulfInfoTracker.Api.AI;

public record ScoringResult(
    int Score,
    string Reasoning,
    IReadOnlyList<string> TopicIds,
    string? NamedEntitiesJson
);

public interface ICredibilityPipeline
{
    Task<ScoringResult> ScoreAsync(Article article, int corroborationCount, CancellationToken ct = default);
}
