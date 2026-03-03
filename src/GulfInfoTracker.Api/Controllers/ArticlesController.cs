using GulfInfoTracker.Api.Data.Entities;
using GulfInfoTracker.Api.Data.Repositories;
using GulfInfoTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace GulfInfoTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController(IArticleRepository repo, IConnectionMultiplexer? redis = null) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ArticleListItem>>> GetArticles(
        [FromQuery] string? topic,
        [FromQuery] string? country,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var cacheKey = $"articles:{page}:{pageSize}:{topic ?? ""}:{country ?? ""}:{q ?? ""}";
        if (redis is not null)
        {
            var db = redis.GetDatabase();
            var cached = await db.StringGetAsync(cacheKey);
            if (cached.HasValue)
            {
                var cachedResult = System.Text.Json.JsonSerializer.Deserialize<PagedResult<ArticleListItem>>(cached!);
                if (cachedResult is not null) return Ok(cachedResult);
            }
        }

        var query = new ArticleQuery(topic, country, q, page, pageSize);
        var result = await repo.QueryAsync(query, ct);

        var items = result.Data.Select(MapToListItem).ToList();
        var pagedResult = new PagedResult<ArticleListItem>(items, result.Total, page, pageSize);

        if (redis is not null)
        {
            var db = redis.GetDatabase();
            await db.StringSetAsync(cacheKey,
                System.Text.Json.JsonSerializer.Serialize(pagedResult),
                TimeSpan.FromMinutes(5));
        }

        return Ok(pagedResult);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleDetail>> GetArticle(Guid id, CancellationToken ct = default)
    {
        var article = await repo.GetByIdAsync(id, ct);
        if (article is null) return NotFound();

        return Ok(MapToDetail(article));
    }

    private static ArticleListItem MapToListItem(Article a) => new(
        a.Id,
        a.PluginId,
        a.HeadlineEn,
        a.HeadlineAr,
        a.SummaryEn,
        a.SummaryAr,
        a.SourceUrl,
        a.PublishedAt,
        a.Country,
        a.CredibilityScore,
        a.FullText,
        a.Translated,
        a.ArticleTopics.Select(at => at.TopicId).ToList()
    );

    private static ArticleDetail MapToDetail(Article a) => new(
        a.Id,
        a.PluginId,
        a.HeadlineEn,
        a.HeadlineAr,
        a.SummaryEn,
        a.SummaryAr,
        a.SourceUrl,
        a.PublishedAt,
        a.IngestedAt,
        a.Country,
        a.CredibilityScore,
        a.CredibilityReasoning,
        a.FullText,
        a.Translated,
        a.ArticleTopics.Select(at => at.TopicId).ToList()
    );
}
