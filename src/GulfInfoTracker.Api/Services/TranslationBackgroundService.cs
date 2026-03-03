using GulfInfoTracker.Api.AI;
using GulfInfoTracker.Api.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GulfInfoTracker.Api.Services;

/// <summary>
/// Background service that translates un-translated articles via the Claude translation agent.
/// Runs every 30 seconds; translates headline and summary EN→AR.
/// </summary>
public class TranslationBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<TranslationBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Translation background service started.");

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await TranslateBatchAsync(stoppingToken);
        }
    }

    private async Task TranslateBatchAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo       = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
        var translator = scope.ServiceProvider.GetRequiredService<ITranslationAgent>();
        var db         = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

        var untranslated = await db.Articles
            .Where(a => !a.Translated && a.HeadlineAr == null)
            .OrderBy(a => a.IngestedAt)
            .Take(10)
            .ToListAsync(ct);

        if (untranslated.Count == 0) return;

        logger.LogInformation("Translating {Count} article(s).", untranslated.Count);

        foreach (var article in untranslated)
        {
            try
            {
                var headlineAr = await translator.TranslateAsync(article.HeadlineEn, "English", "Arabic", ct);
                var summaryAr = article.SummaryEn is not null
                    ? await translator.TranslateAsync(article.SummaryEn, "English", "Arabic", ct)
                    : null;

                await repo.UpdateTranslationAsync(article.Id, headlineAr, summaryAr, ct);
                logger.LogDebug("Translated article {Id}", article.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Translation failed for article {Id}", article.Id);
            }
        }
    }
}
