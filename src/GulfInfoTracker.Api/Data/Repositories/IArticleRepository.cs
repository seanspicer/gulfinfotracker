using GulfInfoTracker.Api.Data.Entities;

namespace GulfInfoTracker.Api.Data.Repositories;

public record ArticleQuery(
    string? Topic = null,
    string? Country = null,
    string? Q = null,
    int Page = 1,
    int PageSize = 20
);

public record ArticleQueryResult(IReadOnlyList<Article> Data, int Total);

public interface IArticleRepository
{
    Task<bool> ExistsByUrlAsync(string url, CancellationToken ct = default);
    Task InsertAsync(Article article, CancellationToken ct = default);
    Task<ArticleQueryResult> QueryAsync(ArticleQuery query, CancellationToken ct = default);
    Task<Article?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Article>> GetUnscoredAsync(int maxAttempts = 3, int batchSize = 20, CancellationToken ct = default);
    Task UpdateScoringAsync(Guid id, int score, string reasoning, IEnumerable<string> topicIds, string? namedEntitiesJson, CancellationToken ct = default);
    Task UpdateTranslationAsync(Guid id, string? headlineAr, string? summaryAr, CancellationToken ct = default);
    Task<int> CountCorroboratingArticlesAsync(Guid excludeId, IEnumerable<string> namedEntities, DateTime publishedAt, CancellationToken ct = default);
}
