using GulfInfoTracker.Api.AI;
using GulfInfoTracker.Api.Data.Repositories;

namespace GulfInfoTracker.Api.Services;

/// <summary>
/// Background service that scores unscored articles via the Claude credibility pipeline.
/// Runs every 1 minute; retries up to 3 times per article on Claude failure.
/// </summary>
public class ScoringBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ScoringBackgroundService> logger) : BackgroundService
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Scoring background service started.");

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ScoreBatchAsync(stoppingToken);
        }
    }

    private async Task ScoreBatchAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo     = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
        var pipeline = scope.ServiceProvider.GetRequiredService<ICredibilityPipeline>();

        var unscored = await repo.GetUnscoredAsync(MaxAttempts, batchSize: 20, ct);
        if (unscored.Count == 0) return;

        logger.LogInformation("Scoring {Count} article(s).", unscored.Count);

        foreach (var article in unscored)
        {
            try
            {
                var corroborationCount = await GetCorroborationCountAsync(repo, article, ct);
                var result = await pipeline.ScoreAsync(article, corroborationCount, ct);

                await repo.UpdateScoringAsync(
                    article.Id,
                    result.Score,
                    result.Reasoning,
                    result.TopicIds,
                    result.NamedEntitiesJson,
                    ct);

                logger.LogInformation("Scored article {Id} → {Score}", article.Id, result.Score);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Scoring failed for article {Id} (attempt {N})", article.Id, article.ScoringAttempts + 1);

                // Increment attempt counter without setting a score; EF direct update
                article.ScoringAttempts += 1;
                await scope.ServiceProvider.GetRequiredService<Data.AppDbContext>().SaveChangesAsync(ct);
            }
        }
    }

    private static async Task<int> GetCorroborationCountAsync(
        IArticleRepository repo,
        Data.Entities.Article article,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(article.NamedEntitiesJson)) return 0;

        List<string>? entities = null;
        try
        {
            entities = System.Text.Json.JsonSerializer.Deserialize<List<string>>(article.NamedEntitiesJson);
        }
        catch { /* ignore parse errors */ }

        if (entities is null || entities.Count == 0) return 0;

        return await repo.CountCorroboratingArticlesAsync(article.Id, entities, article.PublishedAt, ct);
    }
}
