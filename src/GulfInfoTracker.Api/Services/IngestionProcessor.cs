using GulfInfoTracker.Api.Data.Entities;
using GulfInfoTracker.Api.Data.Repositories;
using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Api.Services;

public class IngestionProcessor(
    IArticleRepository articleRepo,
    ISourcePollLogRepository pollLogRepo,
    ILogger<IngestionProcessor> logger) : IIngestionProcessor
{
    public async Task IngestAsync(ArticleCandidate candidate, CancellationToken ct = default)
    {
        if (await articleRepo.ExistsByUrlAsync(candidate.SourceUrl, ct))
        {
            logger.LogDebug("Skipping duplicate: {Url}", candidate.SourceUrl);
            return;
        }

        var article = new Article
        {
            PluginId    = candidate.PluginId,
            HeadlineEn  = candidate.HeadlineEn,
            HeadlineAr  = candidate.HeadlineAr,
            SummaryEn   = candidate.SummaryEn,
            SummaryAr   = candidate.SummaryAr,
            SourceUrl   = candidate.SourceUrl,
            PublishedAt  = candidate.PublishedAt,
            Country     = candidate.Country,
            FullText    = candidate.FullText,
            Translated  = candidate.HeadlineAr is not null,
        };

        await articleRepo.InsertAsync(article, ct);
        logger.LogInformation("Ingested article: {Headline}", candidate.HeadlineEn);
    }

    public async Task IngestPluginAsync(ISourcePlugin plugin, CancellationToken ct = default)
    {
        int ingested = 0;
        string? errorMessage = null;
        bool success = true;

        try
        {
            var rawArticles = await plugin.FetchAsync(ct);
            foreach (var raw in rawArticles)
            {
                var candidate = plugin.ParseArticle(raw);
                if (candidate is not null)
                {
                    await IngestAsync(candidate, ct);
                    ingested++;
                }
            }
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            logger.LogError(ex, "Error polling plugin {PluginId}", plugin.PluginId);
        }

        await pollLogRepo.LogPollAsync(plugin.PluginId, success, ingested, errorMessage, ct);
    }
}
