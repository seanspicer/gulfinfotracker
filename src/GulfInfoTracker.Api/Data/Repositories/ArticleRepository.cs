using GulfInfoTracker.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GulfInfoTracker.Api.Data.Repositories;

public class ArticleRepository(AppDbContext db) : IArticleRepository
{
    public Task<bool> ExistsByUrlAsync(string url, CancellationToken ct = default) =>
        db.Articles.AnyAsync(a => a.SourceUrl == url, ct);

    public async Task InsertAsync(Article article, CancellationToken ct = default)
    {
        db.Articles.Add(article);
        await db.SaveChangesAsync(ct);
    }

    public async Task<ArticleQueryResult> QueryAsync(ArticleQuery query, CancellationToken ct = default)
    {
        var q = db.Articles
            .Include(a => a.ArticleTopics).ThenInclude(at => at.Topic)
            .AsQueryable();

        if (query.ScoredOnly)
            q = q.Where(a => a.CredibilityScore != null);

        if (!string.IsNullOrWhiteSpace(query.Topic))
            q = q.Where(a => a.ArticleTopics.Any(at => at.TopicId == query.Topic));

        if (!string.IsNullOrWhiteSpace(query.Country))
            q = q.Where(a => a.Country.ToLower() == query.Country.ToLower());

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var search = query.Q.ToLower();
            q = q.Where(a =>
                (a.HeadlineEn != null && a.HeadlineEn.ToLower().Contains(search)) ||
                (a.HeadlineAr != null && a.HeadlineAr.ToLower().Contains(search)) ||
                (a.SummaryEn  != null && a.SummaryEn.ToLower().Contains(search)));
        }

        var total = await q.CountAsync(ct);

        var ordered = query.SortBy switch
        {
            "oldest" => q.OrderBy(a => a.PublishedAt),
            "score"  => q.OrderByDescending(a => a.CredibilityScore),
            _        => q.OrderByDescending(a => a.PublishedAt),
        };

        var data = await ordered
                            .Skip((query.Page - 1) * query.PageSize)
                            .Take(query.PageSize)
                            .ToListAsync(ct);

        return new ArticleQueryResult(data, total);
    }

    public Task<Article?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Articles
          .Include(a => a.ArticleTopics).ThenInclude(at => at.Topic)
          .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Article>> GetUnscoredAsync(int maxAttempts = 3, int batchSize = 20, CancellationToken ct = default) =>
        await db.Articles
                .Where(a => a.CredibilityScore == null && a.ScoringAttempts < maxAttempts)
                .OrderBy(a => a.IngestedAt)
                .Take(batchSize)
                .ToListAsync(ct);

    public async Task ResetScoringAttemptsAsync(CancellationToken ct = default) =>
        await db.Articles
                .Where(a => a.CredibilityScore == null && a.ScoringAttempts > 0)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.ScoringAttempts, 0), ct);

    public async Task UpdateScoringAsync(Guid id, int score, string reasoning, IEnumerable<string> topicIds, string? namedEntitiesJson, CancellationToken ct = default)
    {
        var article = await db.Articles.Include(a => a.ArticleTopics).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (article is null) return;

        article.CredibilityScore    = score;
        article.CredibilityReasoning = reasoning;
        article.NamedEntitiesJson   = namedEntitiesJson;

        // Replace topic associations — only valid T1-T5 IDs are allowed
        db.ArticleTopics.RemoveRange(article.ArticleTopics);
        foreach (var topicId in topicIds.Where(t => t is "T1" or "T2" or "T3" or "T4" or "T5"))
        {
            db.ArticleTopics.Add(new ArticleTopic { ArticleId = id, TopicId = topicId });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateTranslationAsync(Guid id, string? headlineAr, string? summaryAr, CancellationToken ct = default)
    {
        var article = await db.Articles.FindAsync([id], ct);
        if (article is null) return;

        article.HeadlineAr = headlineAr;
        article.SummaryAr  = summaryAr;
        article.Translated = true;

        await db.SaveChangesAsync(ct);
    }

    public async Task<int> CountCorroboratingArticlesAsync(Guid excludeId, IEnumerable<string> namedEntities, DateTime publishedAt, CancellationToken ct = default)
    {
        var windowStart = publishedAt.AddHours(-24);
        var windowEnd   = publishedAt.AddHours(24);

        var candidates = await db.Articles
            .Where(a => a.Id != excludeId
                     && a.PublishedAt >= windowStart
                     && a.PublishedAt <= windowEnd
                     && a.NamedEntitiesJson != null)
            .Select(a => new { a.PluginId, a.NamedEntitiesJson })
            .ToListAsync(ct);

        var entities = namedEntities.ToList();
        if (entities.Count == 0) return 0;

        // Count distinct plugins whose articles mention at least one shared named entity
        return candidates
            .Where(c => entities.Any(e => c.NamedEntitiesJson!.Contains(e, StringComparison.OrdinalIgnoreCase)))
            .Select(c => c.PluginId)
            .Distinct()
            .Count();
    }
}
